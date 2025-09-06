using System;
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
        protected int dof = 3;
        /// <summary>
        /// 線形静解析を実行する
        /// </summary>

        public override Result Solve(Model model)
        {
            // 1. グローバル剛性マトリックスの組立
            Matrix<double> globalK = AssembleGlobalStiffness(model);
            // 2. 荷重条件の適用
            Vector<double> globalF = AssembleGlobalLoad(model);
            // 3. 境界条件の適用
            ApplyBoundaryConditions(globalK, globalF, model);
            // 4. 連立方程式の解法
            Vector<double> displacements = solveEquation(globalK, globalF);
            // 5. 結果の格納と返却
            Result result = CreateResult(model, displacements);
            return result;
        }




        #region private Methods

        protected Matrix<double> AssembleGlobalStiffness(Model model) 
        {
            int numNodes = model.Nodes.Count;
            int totalDof = numNodes * dof;
            Matrix<double> globalK = Matrix<double>.Build.Dense(totalDof, totalDof);

            foreach (ElementBase element in model.Elements)
            {
                // 要素を構成するノードのインデックスを取得
                var nodeIndices = element.NodeIds.Select(id => model.Nodes.FindIndex(n => n.Id == id)).ToArray();
                // 要素剛性マトリックスを取得
                Matrix<double> elementGlobalK = element.GlobalStiffness;
                for (int i = 0; i < element.NodeIds.Length; i++)
                {
                    for (int j = 0; j < element.NodeIds.Length; j++)
                    {
                        int nodeI = element.NodeIds[i];
                        int nodeJ = element.NodeIds[j];
                        for (int dofI = 0; dofI < dof; dofI++)
                        {
                            for (int dofJ = 0; dofJ < dof; dofJ++)
                            {
                                int globalI = nodeI * dof + dofI;
                                int globalJ = nodeJ * dof + dofJ;
                                int localI = i * dof + dofI;
                                int localJ = j * dof + dofJ;
                                globalK[globalI, globalJ] += elementGlobalK[localI, localJ];
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
            //節点荷重の組み込み
            foreach (var load in model.Loads)
            {
                //節点荷重の処理
                if (load is PointLoad pointLoad)
                {
                    int nodeIndex = pointLoad.NodeId;
                    int globalIndex = nodeIndex * dof;
                    globalF[globalIndex + 0] += pointLoad.Force.X;
                    globalF[globalIndex + 1] += pointLoad.Force.Y;
                    globalF[globalIndex + 2] += pointLoad.Force.Z;
                    globalF[globalIndex + 3] += pointLoad.Moment.X;
                    globalF[globalIndex + 4] += pointLoad.Moment.Y;
                    globalF[globalIndex + 5] += pointLoad.Moment.Z;
                }
                //要素荷重の処理
                if (load is ElementLoad elementLoad)
                {
                    throw new NotImplementedException();
                }
            }
            return globalF;
        }
        protected void ApplyBoundaryConditions(Matrix<double> globalK, Vector<double> globalF, Model model)
        {
            // 境界条件の適用
            //　既知マトリクスと未知マトリクスを分離して解く方法を採用
            int totalDof = globalK.RowCount;
            int freeDof = totalDof;
            int fixedDof = 0;
            Matrix<double> Kff, Kfc, Kcf, Kcc;
            Vector<double> Ff, Fc;
            List<int> fixedDofIndices = new List<int>();

            foreach (var support in model.Supports)
            {


            }

            throw new NotImplementedException();
        }
        protected Vector<double> solveEquation(Matrix<double> globalK, Vector<double> globalF)
        {
            try
            {
                return globalK.Solve(globalF);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to solve linear system: {ex.Message}", ex);
            }
        }
        protected Result CreateResult(Model model, Vector<double> displacements)
        {
            Result result = new Result();

            result.NodalDisplacements = displacements;
            // 要素応力の計算
            throw new NotImplementedException();
        }

        #endregion



    }
}
