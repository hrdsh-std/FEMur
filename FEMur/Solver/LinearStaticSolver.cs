using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Geometry;
using FEMur.Elements;
using FEMur.Models;
using FEMur.Results;
using FEMur.Loads;
using FEMur.Joints;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Solver
{
    public class LinearStaticSolver : Solver
    {
        protected int dof = 6;

        // 正則化設定（既存の明示的制御用）
        public bool EnableRegularization { get; set; } = false;
        public double RotationalRegularizationFactor { get; set; } = 1e-9;

        public bool EnableTranslationalRegularization { get; set; } = false;
        public double TranslationalRegularizationFactor { get; set; } = 1e-10;

        // 追加: 自動正則化の有効化（デフォルトtrue）
        public bool EnableAutoRegularization { get; set; } = true;

        // 追加: 解析結果の警告フラグ
        public List<string> Warnings { get; private set; } = new List<string>();

        public override Result Solve(Model model)
        {
            // 警告をクリア
            Warnings.Clear();

            Matrix<double> globalK = AssembleGlobalStiffness(model);
            ApplySupportSprings(model, globalK);

            Vector<double> globalF = AssembleGlobalLoad(model);
            Vector<double> displacements = solve(globalK, globalF, model);

            var result = new Result
            {
                NodalDisplacements = displacements,
                ElementStresses = CalcElementStresses(model, displacements),
                ReactionForces = CalcReactionForces(model, globalK, globalF, displacements)
            };

            // 追加: Modelに結果を格納
            model.Result = result;
            model.IsSolved = true;

            return result;
        }

        public Vector<double> solveDisp(Model model)
        {
            // 警告をクリア
            Warnings.Clear();

            Matrix<double> globalK = AssembleGlobalStiffness(model);
            ApplySupportSprings(model, globalK);

            Vector<double> globalF = AssembleGlobalLoad(model);
            return solve(globalK, globalF, model);
        }

        /// <summary>
        /// 反力を計算
        /// R = K × U - F (拘束自由度のみ)
        /// </summary>
        protected List<ReactionForce> CalcReactionForces(Model model, Matrix<double> globalK, 
            Vector<double> globalF, Vector<double> displacements)
        {
            var reactionForces = new List<ReactionForce>();

            // 内力ベクトルを計算: F_internal = K × U
            Vector<double> internalForces = globalK * displacements;

            // 反力 = 内力 - 外力
            Vector<double> reactions = internalForces - globalF;

            // 拘束された節点ごとに反力を集計
            foreach (var support in model.Supports)
            {
                int nodeIndex = model.Nodes.FindIndex(n => n.Id == support.NodeId);
                if (nodeIndex < 0)
                    continue;

                int baseIndex = nodeIndex * dof;

                double fx = 0.0, fy = 0.0, fz = 0.0;
                double mx = 0.0, my = 0.0, mz = 0.0;

                if (support.Conditions[0]) fx = reactions[baseIndex + 0];
                if (support.Conditions[1]) fy = reactions[baseIndex + 1];
                if (support.Conditions[2]) fz = reactions[baseIndex + 2];
                if (support.Conditions[3]) mx = reactions[baseIndex + 3];
                if (support.Conditions[4]) my = reactions[baseIndex + 4];
                if (support.Conditions[5]) mz = reactions[baseIndex + 5];

                if (Math.Abs(fx) > 1e-12 || Math.Abs(fy) > 1e-12 || Math.Abs(fz) > 1e-12 ||
                    Math.Abs(mx) > 1e-12 || Math.Abs(my) > 1e-12 || Math.Abs(mz) > 1e-12)
                {
                    reactionForces.Add(new ReactionForce(support.NodeId, fx, fy, fz, mx, my, mz));
                }
            }

            return reactionForces;
        }

        /// <summary>
        /// 全要素の断面力(応力)を計算
        /// </summary>
        protected List<Results.ElementStress> CalcElementStresses(Model model, Vector<double> globalDisplacements)
        {
            var elementStresses = new List<Results.ElementStress>();

            // 要素IDからJointを取得するための辞書を作成
            var jointByElementId = model.Joints?
                .Where(j => j != null && j.ElementId >= 0)
                .ToDictionary(j => j.ElementId, j => j)
                ?? new Dictionary<int, Joint>();

            foreach (var element in model.Elements)
            {
                if (element is BeamElement beamElement)
                {
                    // 要素の節点IDから節点インデックスを取得
                    var nodeIndices = element.NodeIds
                        .Select(id => model.Nodes.FindIndex(n => n.Id == id))
                        .ToArray();

                    // グローバル変位から要素の変位を抽出
                    var elementDispGlobal = Vector<double>.Build.Dense(12);
                    for (int i = 0; i < 2; i++) // 2節点要素
                    {
                        int nodeIdx = nodeIndices[i];
                        for (int d = 0; d < dof; d++)
                        {
                            elementDispGlobal[i * dof + d] = globalDisplacements[nodeIdx * dof + d];
                        }
                    }

                    // グローバル変位を局所変位に変換（Tの転置を使用）
                    Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                    Vector<double> elementDispLocal = T.Transpose() * elementDispGlobal;

                    // 局所剛性マトリクスを取得（Jointがある場合は縮約後のものを使用）
                    Matrix<double> keLocal = element.CalcLocalStiffness(model.Nodes);
                    if (jointByElementId.TryGetValue(element.Id, out Joint joint))
                    {
                        keLocal = ApplyJointStaticCondensation(keLocal, joint, element);
                    }

                    // 断面力を計算（縮約後の剛性マトリクスを使用）
                    var stress = CalcElementStressWithMatrix(beamElement, elementDispLocal, keLocal);
                    elementStresses.Add(stress);
                }
            }

            //j端の断面力を負に反転
            foreach (var stress in elementStresses)
            {
                stress.Fx_j = -stress.Fx_j;
                stress.Fy_j = -stress.Fy_j;
                stress.Fz_j = -stress.Fz_j;
                stress.Mx_j = -stress.Mx_j;
                stress.My_j = -stress.My_j;
                stress.Mz_j = -stress.Mz_j;
            }

            return elementStresses;
        }

        /// <summary>
        /// 指定された剛性マトリクスを使用して断面力を計算
        /// </summary>
        private Results.ElementStress CalcElementStressWithMatrix(BeamElement element, Vector<double> localDisplacements, Matrix<double> keLocal)
        {
            // 局所座標系の断面力: f_local = K_local * u_local
            Vector<double> forces = keLocal * localDisplacements;

            var stress = new Results.ElementStress(element.Id);

            // i端（節点1）の断面力
            stress.Fx_i = forces[0];  // 軸力
            stress.Fy_i = forces[1];  // せん断力Y
            stress.Fz_i = forces[2];  // せん断力Z
            stress.Mx_i = forces[3];  // ねじりモーメント
            stress.My_i = forces[4];  // 曲げモーメントY
            stress.Mz_i = forces[5];  // 曲げモーメントZ

            // j端（節点2）の断面力
            stress.Fx_j = forces[6];
            stress.Fy_j = forces[7];
            stress.Fz_j = forces[8];
            stress.Mx_j = forces[9];
            stress.My_j = forces[10];
            stress.Mz_j = forces[11];

            return stress;
        }

        #region private Methods

        protected Matrix<double> AssembleGlobalStiffness(Model model)
        {
            int numNodes = model.Nodes.Count;
            int totalDof = numNodes * dof;
            Matrix<double> globalK = Matrix<double>.Build.Dense(totalDof, totalDof);

            // 要素IDからJointを取得するための辞書を作成
            var jointByElementId = model.Joints?
                .Where(j => j != null && j.ElementId >= 0)
                .ToDictionary(j => j.ElementId, j => j)
                ?? new Dictionary<int, Joint>();

            foreach (ElementBase element in model.Elements)
            {
                var nodeIndices = element.NodeIds
                    .Select(id => model.Nodes.FindIndex(n => n.Id == id))
                    .ToArray();

                Matrix<double> keLocal = element.CalcLocalStiffness(model.Nodes);

                // Jointが存在する場合は静的縮約を適用
                if (jointByElementId.TryGetValue(element.Id, out Joint joint))
                {
                    keLocal = ApplyJointStaticCondensation(keLocal, joint, element);
                }

                Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                Matrix<double> elementK = T * keLocal * T.Transpose();

                for (int i = 0; i < element.NodeIds.Count; i++)
                {
                    for (int j = 0; j < element.NodeIds.Count; j++)
                    {
                        int nodeI = nodeIndices[i];
                        int nodeJ = nodeIndices[j];
                        for (int dofI = 0; dofI < dof; dofI++)
                        {
                            for (int dofJ = 0; dofJ < dof; dofJ++)
                            {
                                int globalI = nodeI * dof + dofI;
                                int globalJ = nodeJ * dof + dofJ;
                                int localI = i * dof + dofI;
                                int localJ = j * dof + dofJ;
                                globalK[globalI, globalJ] += elementK[localI, localJ];
                            }
                        }
                    }
                }
            }
            return globalK;
        }

        /// <summary>
        /// 材端ばね（Joint）を考慮した静的縮約を適用
        /// K_eq = K_rr - K_ri * K_ii^(-1) * K_ir
        /// </summary>
        /// <param name="keLocal">元の局所剛性マトリクス (12x12)</param>
        /// <param name="joint">接合条件</param>
        /// <param name="element">要素</param>
        /// <returns>縮約後の剛性マトリクス (12x12)</returns>
        private Matrix<double> ApplyJointStaticCondensation(Matrix<double> keLocal, Joint joint, ElementBase element)
        {
            // 両端とも剛結の場合は縮約不要
            if (joint.IsStartRigid() && joint.IsEndRigid())
            {
                return keLocal;
            }

            // 内部自由度（ばねで接続される自由度）のインデックスを特定
            var internalDofs = new List<int>();
            var retainedDofs = new List<int>();

            // Start端（i端）の自由度チェック: 0-5
            for (int i = 0; i < Joint.DOFS_PER_END; i++)
            {
                if (!joint.IsFixed[i])
                {
                    // 固定されていない = ばねまたは自由 → 内部自由度として扱う
                    internalDofs.Add(i);
                }
                else
                {
                    retainedDofs.Add(i);
                }
            }

            // End端（j端）の自由度チェック: 6-11
            for (int i = Joint.DOFS_PER_END; i < Joint.TOTAL_DOFS; i++)
            {
                if (!joint.IsFixed[i])
                {
                    internalDofs.Add(i);
                }
                else
                {
                    retainedDofs.Add(i);
                }
            }

            // 内部自由度がない場合は縮約不要
            if (internalDofs.Count == 0)
            {
                return keLocal;
            }

            // 拡張剛性マトリクスを構築（材端ばねを追加）
            // 元の12x12マトリクスに内部自由度分を追加
            int originalSize = 12;
            int internalCount = internalDofs.Count;
            int expandedSize = originalSize + internalCount;

            var kExpanded = Matrix<double>.Build.Dense(expandedSize, expandedSize);

            // 元の剛性マトリクスをコピー
            for (int i = 0; i < originalSize; i++)
            {
                for (int j = 0; j < originalSize; j++)
                {
                    int i2 = i;
                    int j2 = j;

                    if(internalDofs.Contains(i))
                    {
                        i2 = originalSize + internalDofs.IndexOf(i);
                    }
                    if(internalDofs.Contains(j))
                    {
                        j2 = originalSize + internalDofs.IndexOf(j);
                    }

                    kExpanded[i2, j2] = keLocal[i, j];
                }
            }

            // 材端ばねの剛性を追加
            for (int idx = 0; idx < internalCount; idx++)
            {
                int dofIdx = internalDofs[idx];
                double springStiffness = GetSpringStiffness(joint, dofIdx);

                // ばね剛性が0または負の場合はピン接合として扱う（非常に小さい値を設定）
                if (springStiffness <= 0)
                {
                    springStiffness = 1.0E-5; // 数値安定性のための微小値
                }

                // 対角成分にばね剛性を加算
                kExpanded[dofIdx, dofIdx] += springStiffness;

                // 内部自由度の対角成分
                int internalIdx = originalSize + idx;
                kExpanded[internalIdx, internalIdx] += springStiffness;

                // 結合項（off-diagonal）
                kExpanded[dofIdx, internalIdx] = -springStiffness;
                kExpanded[internalIdx, dofIdx] = -springStiffness;
            }

            // 静的縮約を実行
            // K を [K_rr, K_ri; K_ir, K_ii] に分割
            // retained: 0-11 (元の自由度), internal: 12以降（内部自由度）


            var kRR = kExpanded.SubMatrix(0, originalSize, 0, originalSize);
            var kRI = kExpanded.SubMatrix(0, originalSize, originalSize, internalCount);
            var kIR = kExpanded.SubMatrix(originalSize, internalCount, 0, originalSize);
            var kII = kExpanded.SubMatrix(originalSize, internalCount, originalSize, internalCount);

            // K_eq = K_rr - K_ri * K_ii^(-1) * K_ir
            Matrix<double> kIIinv;
            try
            {
                kIIinv = kII.Inverse();
            }
            catch
            {
                // 逆行列が計算できない場合は警告を出して元のマトリクスを返す
                Warnings.Add($"Joint condensation failed for Element {element.Id}: K_ii is singular. Using original stiffness.");
                return keLocal;
            }

            var kEq = kRR - kRI * kIIinv * kIR;

            return kEq;
        }

        /// <summary>
        /// Jointから指定された自由度のばね剛性を取得
        /// </summary>
        private double GetSpringStiffness(Joint joint, int dofIndex)
        {
            bool isStartEnd = dofIndex < Joint.DOFS_PER_END;
            int localIdx = isStartEnd ? dofIndex : dofIndex - Joint.DOFS_PER_END;

            if (localIdx < 3)
            {
                // 並進自由度 (DX, DY, DZ)
                return isStartEnd
                    ? joint.StartTranslationalStiffness[localIdx]
                    : joint.EndTranslationalStiffness[localIdx];
            }
            else
            {
                // 回転自由度 (RX, RY, RZ)
                int rotIdx = localIdx - 3;
                return isStartEnd
                    ? joint.StartRotationalStiffness[rotIdx]
                    : joint.EndRotationalStiffness[rotIdx];
            }
        }

        protected Vector<double> AssembleGlobalLoad(Model model)
        {
            int numNodes = model.Nodes.Count;
            int totalDof = numNodes * dof;
            Vector<double> globalF = Vector<double>.Build.Dense(totalDof);

            foreach (var load in model.Loads)
            {
                if (load is PointLoad pointLoad)
                {
                    int nodeIndex = model.Nodes.FindIndex(n => n.Id == pointLoad.NodeId);
                    if (nodeIndex < 0)
                        throw new ArgumentException($"PointLoad refers to unknown NodeId={pointLoad.NodeId}");
                    
                    int globalIndex = nodeIndex * dof;
                    
                    globalF[globalIndex + 0] += pointLoad.Force.X;
                    globalF[globalIndex + 1] += pointLoad.Force.Y;
                    globalF[globalIndex + 2] += pointLoad.Force.Z;
                    globalF[globalIndex + 3] += pointLoad.Moment.X;
                    globalF[globalIndex + 4] += pointLoad.Moment.Y;
                    globalF[globalIndex + 5] += pointLoad.Moment.Z;
                }
                else if (load is ElementLoad elementLoad)
                {
                    var element = model.Elements.FirstOrDefault(e => e.Id == elementLoad.ElementId);
                    if (element == null)
                        throw new ArgumentException($"ElementLoad refers to unknown ElementId={elementLoad.ElementId}");

                    var nodeIndices = element.NodeIds
                        .Select(id => model.Nodes.FindIndex(n => n.Id == id))
                        .ToArray();

                    Vector<double> feLocal = elementLoad.CalcEquivalentNodalLoadLocal(element, model.Nodes);
                    
                    Vector<double> feGlobal;
                    
                    if (elementLoad.Local)
                    {
                        Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                        feGlobal = T * feLocal;
                    }
                    else
                    {
                        if (element is LineElement lineElement &&
                            lineElement.LocalAxisX != null &&
                            lineElement.LocalAxisY != null &&
                            lineElement.LocalAxisZ != null)
                        {
                            var R = Matrix<double>.Build.DenseOfRowArrays(
                                lineElement.LocalAxisX,
                                lineElement.LocalAxisY,
                                lineElement.LocalAxisZ
                            );
                            
                            var qGlobal = Vector<double>.Build.DenseOfArray(new[] 
                            { 
                                elementLoad.QLocal.X,
                                elementLoad.QLocal.Y, 
                                elementLoad.QLocal.Z 
                            });
                            
                            var qLocal = R * qGlobal;

                            var tempElementLoad = new ElementLoad(
                                elementLoad.ElementId,
                                new Vector3(qLocal[0], qLocal[1], qLocal[2]),
                                elementLoad.MLocal,
                                true
                            );
                            
                            feLocal = tempElementLoad.CalcEquivalentNodalLoadLocal(element, model.Nodes);
                            
                            Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                            feGlobal = T * feLocal;
                        }
                        else
                        {
                            Warnings.Add($"ElementLoad for ElementId={elementLoad.ElementId} is marked as Global, " +
                                       $"but element local coordinate system is not defined. " +
                                       $"Treating as Local.");
                    
                            Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                            feGlobal = T * feLocal;
                        }
                    }

                    for (int i = 0; i < element.NodeIds.Count; i++)
                    {
                        int nodeI = nodeIndices[i];
                        int baseIndex = nodeI * dof;
                        for (int d = 0; d < dof; d++)
                        {
                            globalF[baseIndex + d] += feGlobal[i * dof + d];
                        }
                    }
                }
            }
            return globalF;
        }

        protected Vector<double> solve(Matrix<double> globalK, Vector<double> globalF, Model model)
        {
            var totalDof = globalK.RowCount;
            var fixedDof = new HashSet<int>();

            // 支持条件 → 拘束DOF
            foreach (var support in model.Supports)
            {
                int nodeIndex = model.Nodes.FindIndex(n => n.Id == support.NodeId);
                if (nodeIndex < 0)
                    throw new ArgumentException($"Support refers to unknown NodeId={support.NodeId}");
                int baseIndex = nodeIndex * dof;
                for (int i = 0; i < dof; i++)
                {
                    if (support.Conditions[i]) fixedDof.Add(baseIndex + i);
                }
            }

            int[] freeDof = Enumerable.Range(0, totalDof).Where(i => !fixedDof.Contains(i)).ToArray();
            Matrix<double> kff = Matrix<double>.Build.Dense(freeDof.Length, freeDof.Length);
            Vector<double> ff = Vector<double>.Build.Dense(freeDof.Length);

            for (int i = 0; i < freeDof.Length; i++)
            {
                ff[i] = globalF[freeDof[i]];
                for (int j = 0; j < freeDof.Length; j++)
                {
                    kff[i, j] = globalK[freeDof[i], freeDof[j]];
                }
            }

            // 明示的な正則化が有効な場合は適用
            if (EnableRegularization)
            {
                ApplyRotationalRegularization(kff, freeDof, model);
            }
            if (EnableTranslationalRegularization)
            {
                ApplyTranslationalRegularization(kff, freeDof, model);
            }

            // ランク検査(SVD)
            bool isSingular = false;
            if (freeDof.Length > 0)
            {
                var svd = kff.Svd(true);
                var s = svd.S;
                double smax = s.AbsoluteMaximum();
                double tol = Math.Max(kff.RowCount, kff.ColumnCount) * 1e-16 * (smax <= 0 ? 1.0 : smax);
                int rank = s.Count(v => v > tol);
                
                if (rank < kff.RowCount)
                {
                    isSingular = true;
                    var V = svd.VT.Transpose();
                    var nullModes = new List<string>();
                    int modesToReport = Math.Min(6, kff.RowCount - rank);
                    var sIndexed = s.Select((val, idx) => new { val, idx }).OrderBy(x => x.val).Take(modesToReport).ToList();
                    foreach (var m in sIndexed)
                    {
                        var v = V.Column(m.idx);
                        var vAbs = v.Map(Math.Abs);
                        var top = vAbs.Select((val, idx) => new { val, idx })
                                      .OrderByDescending(x => x.val)
                                      .Take(5)
                                      .Select(x => GlobalDofName(freeDof[x.idx], model));
                        nullModes.Add(string.Join(", ", top));
                    }

                    // 自動正則化が有効で、まだ正則化していない場合
                    if (EnableAutoRegularization && !EnableRegularization && !EnableTranslationalRegularization)
                    {
                        string warningMsg = $"Singular stiffness matrix detected (rank={rank}, n={kff.RowCount}). " +
                            $"Auto-regularization applied. Free modes around: [{string.Join("] | [", nullModes)}]. " +
                            $"Consider adding appropriate supports or constraints.";
                        Warnings.Add(warningMsg);

                        // 自動正則化を適用
                        ApplyRotationalRegularization(kff, freeDof, model);
                        ApplyTranslationalRegularization(kff, freeDof, model);

                        // 正則化後に再度ランク検査
                        var svd2 = kff.Svd(true);
                        var s2 = svd2.S;
                        double smax2 = s2.AbsoluteMaximum();
                        double tol2 = Math.Max(kff.RowCount, kff.ColumnCount) * 1e-16 * (smax2 <= 0 ? 1.0 : smax2);
                        int rank2 = s2.Count(v => v > tol2);

                        if (rank2 < kff.RowCount)
                        {
                            // 正則化後も特異の場合はエラー
                            throw new InvalidOperationException(
                                $"Stiffness matrix remains singular after auto-regularization (rank={rank2}, n={kff.RowCount}). " +
                                $"Free modes: [{string.Join("] | [", nullModes)}]. " +
                                $"Please add proper supports or enable manual regularization with higher factors.");
                        }
                    }
                    else if (!EnableAutoRegularization)
                    {
                        // 自動正則化が無効の場合はエラーを投げる
                        throw new InvalidOperationException(
                            $"Stiffness submatrix is singular or ill-conditioned. " +
                            $"rank={rank}, n={kff.RowCount}. " +
                            $"Likely free rigid-body modes around: [{string.Join("] | [", nullModes)}]. " +
                            $"Adjust supports or enable regularization (rotational/translational).");
                    }
                }
            }

            Vector<double> displacements = Vector<double>.Build.Dense(totalDof);
            var freeDisplacements = kff.Solve(ff);

            if (freeDisplacements.Any(d => double.IsNaN(d) || double.IsInfinity(d)))
            {
                throw new InvalidOperationException("Solver produced NaN/Infinity in displacement vector. " +
                    "This typically indicates singular stiffness or invalid element geometry.");
            }

            for (int i = 0; i < freeDof.Length; i++)
            {
                displacements[freeDof[i]] = freeDisplacements[i];
            }
            return displacements;
        }

        private void ApplySupportSprings(Model model, Matrix<double> globalK)
        {
            for (int s = 0; s < model.Supports.Count; s++)
            {
                var sup = model.Supports[s];
                int nodeIndex = model.Nodes.FindIndex(n => n.Id == sup.NodeId);
                if (nodeIndex < 0) throw new ArgumentException($"Support refers to unknown NodeId={sup.NodeId}");
                int baseIdx = nodeIndex * dof;
                for (int i = 0; i < dof; i++)
                {
                    double k = sup.Stiffness[i];
                    if (k > 0.0)
                    {
                        globalK[baseIdx + i, baseIdx + i] += k;
                    }
                }
            }
        }

        // 回転自由度の正則化(対象: kff)
        private void ApplyRotationalRegularization(Matrix<double> kff, int[] freeDof, Model model)
        {
            double kref = ComputeReferenceRotationalStiffness(model);
            double kreg = kref * RotationalRegularizationFactor;
            if (kreg <= 0.0) return;

            for (int i = 0; i < freeDof.Length; i++)
            {
                int local = freeDof[i] % dof;
                if (local >= 3 && local <= 5) // RX, RY, RZ
                {
                    kff[i, i] += kreg;
                }
            }
        }

        // 並進自由度の正則化(対象: kff)
        private void ApplyTranslationalRegularization(Matrix<double> kff, int[] freeDof, Model model)
        {
            double kref = ComputeReferenceTranslationalStiffness(model);
            double kreg = kref * TranslationalRegularizationFactor;
            if (kreg <= 0.0) return;

            for (int i = 0; i < freeDof.Length; i++)
            {
                int local = freeDof[i] % dof;
                if (local >= 0 && local <= 2) // UX, UY, UZ
                {
                    kff[i, i] += kreg;
                }
            }
        }

        // 代表回転剛性: 中央値(EIyy/L, EIzz/L, GJ/L)
        private double ComputeReferenceRotationalStiffness(Model model)
        {
            var values = new List<double>();
            foreach (var e in model.Elements)
            {
                if (e is BeamElement be)
                {
                    var n1 = model.Nodes.First(n => n.Id == be.NodeIds[0]);
                    var n2 = model.Nodes.First(n => n.Id == be.NodeIds[1]);
                    double dx = n2.Position.X - n1.Position.X;
                    double dy = n2.Position.Y - n1.Position.Y;
                    double dz = n2.Position.Z - n1.Position.Z;
                    double L = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (L <= 0) continue;

                    double E = be.Material.E;
                    double G = be.Material.G;
                    var cs = (FEMur.CrossSections.CrossSection_Beam)be.CrossSection;

                    if (cs.Iyy > 0) values.Add(E * cs.Iyy / L);
                    if (cs.Izz > 0) values.Add(E * cs.Izz / L);
                    if (cs.J > 0) values.Add(G * cs.J / L);
                }
            }
            if (values.Count == 0) return 1.0;
            values.Sort();
            return values[values.Count / 2];
        }

        // 代表並進剛性: 中央値(EA/L)
        private double ComputeReferenceTranslationalStiffness(Model model)
        {
            var values = new List<double>();
            foreach (var e in model.Elements)
            {
                if (e is BeamElement be)
                {
                    var n1 = model.Nodes.First(n => n.Id == be.NodeIds[0]);
                    var n2 = model.Nodes.First(n => n.Id == be.NodeIds[1]);
                    double dx = n2.Position.X - n1.Position.X;
                    double dy = n2.Position.Y - n1.Position.Y;
                    double dz = n2.Position.Z - n1.Position.Z;
                    double L = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (L <= 0) continue;

                    double E = be.Material.E;
                    var cs = (FEMur.CrossSections.CrossSection_Beam)be.CrossSection;

                    if (cs.A > 0) values.Add(E * cs.A / L);
                }
            }
            if (values.Count == 0) return 1.0;
            values.Sort();
            return values[values.Count / 2];
        }

        #endregion

        private string GlobalDofName(int gidx, Model model)
        {
            int nodeIndex = gidx / dof;
            int dofLocal = gidx % dof;
            var node = model.Nodes[nodeIndex];
            string[] names = { "UX", "UY", "UZ", "RX", "RY", "RZ" };
            return $"NodeId={node.Id}:{names[dofLocal]}";
        }
    }
}
