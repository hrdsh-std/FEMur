using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Models;

namespace FEMurGH.Solver
{
    public class LinearStaticSolver : GH_Component
    {
        public LinearStaticSolver()
          : base("LinearStaticSolver(FEMur)", "LinearStaticSolver",
              "Solve linear static FEM analysis (modifies Model in-place)",
              "FEMur", "Solver")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "FEMur Model to solve", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Execute solver", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model with computed results", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "Solver information", GH_ParamAccess.item);
            pManager.AddTextParameter("Warnings", "W", "Solver warnings (if any)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Model model = null;
            bool run = false;

            if (!DA.GetData(0, ref model))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Model is required");
                return;
            }

            if (!DA.GetData(1, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Run flag not set");
            }

            string info = "";
            var warnings = new List<string>();

            if (!run)
            {
                info = "Solver not executed. Set Run = true to solve.";
                if (model.IsSolved)
                {
                    info += "\nModel has previously computed results.";
                }
                DA.SetData(0, model);
                DA.SetData(1, info);
                DA.SetDataList(2, warnings);
                return;
            }

            try
            {
                // ソルバーインスタンスを作成（デフォルト設定: 自動正則化有効）
                var solver = new FEMur.Solver.LinearStaticSolver();

                // 解析実行（Modelが直接更新される)
                solver.Solve(model);

                // 警告を取得
                if (solver.Warnings.Count > 0)
                {
                    warnings.AddRange(solver.Warnings);
                    foreach (var warning in solver.Warnings)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                }

                // 情報メッセージ作成
                info = $"Analysis completed successfully.\n" +
                       $"Nodes: {model.Nodes.Count}\n" +
                       $"Elements: {model.Elements.Count}\n" +
                       $"Supports: {model.Supports.Count}\n" +
                       $"Loads: {model.Loads.Count}\n" +
                       $"Total DOF: {model.Nodes.Count * 6}\n" +
                       $"IsSolved: {model.IsSolved}";

                if (warnings.Count > 0)
                {
                    info += $"\n\nWarnings: {warnings.Count} (see Warnings output for details)";
                }

                DA.SetData(0, model);
                DA.SetData(1, info);
                DA.SetDataList(2, warnings);
            }
            catch (InvalidOperationException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Solver error: {ex.Message}");
                DA.SetData(0, model);
                DA.SetData(1, $"Error: {ex.Message}");
                DA.SetDataList(2, warnings);
            }
            catch (ArgumentException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Model error: {ex.Message}");
                DA.SetData(0, model);
                DA.SetData(1, $"Error: {ex.Message}");
                DA.SetDataList(2, warnings);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
                DA.SetData(0, model);
                DA.SetData(1, $"Error: {ex.Message}");
                DA.SetDataList(2, warnings);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("2CB1CA46-F30B-4E0B-8C2E-30AFF3AB920B");
    }
}