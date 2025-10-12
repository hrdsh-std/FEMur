﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Elements;
using FEMur.Models;
using FEMur.Results;
using FEMur.Loads;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Solver
{
    public class LinearStaticSolver : Solver
    {
        protected int dof = 6;

        // 正則化（既存）
        public bool EnableRegularization { get; set; } = false;
        public double RotationalRegularizationFactor { get; set; } = 1e-9;

        // 追加: 並進自由度の正則化
        public bool EnableTranslationalRegularization { get; set; } = false;
        public double TranslationalRegularizationFactor { get; set; } = 1e-10;

        public override Result Solve(Model model)
        {
            Matrix<double> globalK = AssembleGlobalStiffness(model);
            ApplySupportSprings(model, globalK);

            Vector<double> globalF = AssembleGlobalLoad(model);
            Vector<double> displacements = solve(globalK, globalF, model);

            var result = new Result
            {
                NodalDisplacements = displacements,
                ElementStresses = CalcElementStresses(model, displacements)
            };

            return result;
        }

        public Vector<double> solveDisp(Model model)
        {
            Matrix<double> globalK = AssembleGlobalStiffness(model);
            ApplySupportSprings(model, globalK);

            Vector<double> globalF = AssembleGlobalLoad(model);
            return solve(globalK, globalF, model);
        }

        /// <summary>
        /// 全要素の断面力（応力）を計算
        /// </summary>
        protected List<Results.ElementStress> CalcElementStresses(Model model, Vector<double> globalDisplacements)
        {
            var elementStresses = new List<Results.ElementStress>();

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

                    // グローバル変位を局所変位に変換
                    Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                    Vector<double> elementDispLocal = T * elementDispGlobal;

                    // 断面力を計算
                    var stress = beamElement.CalcElementStress(elementDispLocal, model.Nodes);
                    elementStresses.Add(stress);
                }
            }

            return elementStresses;
        }

        #region private Methods

        protected Matrix<double> AssembleGlobalStiffness(Model model)
        {
            int numNodes = model.Nodes.Count;
            int totalDof = numNodes * dof;
            Matrix<double> globalK = Matrix<double>.Build.Dense(totalDof, totalDof);

            foreach (ElementBase element in model.Elements)
            {
                var nodeIndices = element.NodeIds
                    .Select(id => model.Nodes.FindIndex(n => n.Id == id))
                    .ToArray();

                Matrix<double> keLocal = element.CalcLocalStiffness(model.Nodes);
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
                    Matrix<double> T = element.CalcTransformationMatrix(model.Nodes);
                    Vector<double> feGlobal = T * feLocal;

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

            // Kff に対する正則化（回転/並進）をここで適用
            if (EnableRegularization)
            {
                ApplyRotationalRegularization(kff, freeDof, model);
            }
            if (EnableTranslationalRegularization)
            {
                ApplyTranslationalRegularization(kff, freeDof, model);
            }

            // ランク検査（SVD）
            if (freeDof.Length > 0)
            {
                var svd = kff.Svd(true);
                var s = svd.S;
                double smax = s.AbsoluteMaximum();
                double tol = Math.Max(kff.RowCount, kff.ColumnCount) * 1e-16 * (smax <= 0 ? 1.0 : smax);
                int rank = s.Count(v => v > tol);
                if (rank < kff.RowCount)
                {
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

                    throw new InvalidOperationException(
                        $"Stiffness submatrix is singular or ill-conditioned. " +
                        $"rank={rank}, n={kff.RowCount}. " +
                        $"Likely free rigid-body modes around: [{string.Join("] | [", nullModes)}]. " +
                        $"Adjust supports or enable regularization (rotational/translational).");
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

        // 回転自由度の正則化（対象: kff）
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

        // 並進自由度の正則化（対象: kff）
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
