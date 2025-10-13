using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Results;
using FEMur.Models;
using FEMur.Elements;
using MathNet.Numerics.LinearAlgebra;

namespace FEMurGH.Results
{
    public class ResultView : GH_Component
    {
        public ResultView()
          : base("ResultView(FEMur)", "ResultView",
              "Visualize FEMur analysis results in Rhino space (deformed model)",
              "FEMur", "Results")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model with computed results", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "Deformation scale factor", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("DeformedModel", "DM", "Deformed model as lines", GH_ParamAccess.list);
            pManager.AddPointParameter("DeformedNodes", "DN", "Deformed node positions", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Model model = null;
            double scale = 1.0;

            if (!DA.GetData(0, ref model))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "AnalyzedModel is required");
                return;
            }

            if (!model.IsSolved || model.Result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Model has not been solved yet. Run LinearStaticSolver first.");
                return;
            }

            if (!DA.GetData(1, ref scale)) return;

            try
            {
                var result = model.Result;
                var displacements = result.NodalDisplacements;
                
                if (displacements == null || displacements.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No displacement data in result");
                    return;
                }

                // 変形後の節点座標を計算
                var deformedNodes = new List<Point3d>(model.Nodes.Count);
                int dof = 6;

                for (int i = 0; i < model.Nodes.Count; i++)
                {
                    var node = model.Nodes[i];
                    double ux = displacements[i * dof + 0] * scale;
                    double uy = displacements[i * dof + 1] * scale;
                    double uz = displacements[i * dof + 2] * scale;

                    var deformedPos = new Point3d(
                        node.Position.X + ux,
                        node.Position.Y + uy,
                        node.Position.Z + uz
                    );
                    deformedNodes.Add(deformedPos);
                }

                // 変形後の要素（線分）を作成
                var deformedLines = new List<Line>();
                foreach (var element in model.Elements)
                {
                    if (element is BeamElement || element is LineElement)
                    {
                        // 2節点要素を想定
                        if (element.NodeIds.Count >= 2)
                        {
                            int nodeId1 = element.NodeIds[0];
                            int nodeId2 = element.NodeIds[1];

                            // NodeId から Node インデックスを取得
                            int idx1 = model.Nodes.FindIndex(n => n.Id == nodeId1);
                            int idx2 = model.Nodes.FindIndex(n => n.Id == nodeId2);

                            if (idx1 >= 0 && idx2 >= 0 && idx1 < deformedNodes.Count && idx2 < deformedNodes.Count)
                            {
                                var line = new Line(deformedNodes[idx1], deformedNodes[idx2]);
                                deformedLines.Add(line);
                            }
                        }
                    }
                }

                DA.SetDataList(0, deformedLines);
                DA.SetDataList(1, deformedNodes);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error visualizing result: {ex.Message}");
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("BF844576-0984-4565-BE48-EA6B6774E3FF");
    }
}