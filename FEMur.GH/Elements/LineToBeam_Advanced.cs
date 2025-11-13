using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

// FEMur 型のエイリアス
using FEMNode = FEMur.Nodes.Node;
using FEMMat = FEMur.Materials.Material;
using FEMCS = FEMur.CrossSections.CrossSection_Beam;
using FEMBeamElement = FEMur.Elements.BeamElement;
using FEMur.Geometry.Intermediate;
using FEMPoint3 = FEMur.Geometry.Point3;

namespace FEMurGH.Elements
{
    /// <summary>
    /// 中間クラスを使用した LineToBeam 変換コンポーネント
    /// 
    /// 入力: List<Line> (Rhino lines), Material, CrossSection_Beam, BetaAngle
    /// 出力: List<Node>, List<BeamElement>
    /// 
    /// 特徴：
    /// - 自動的にノード重複を排除
    /// - 自動的に Node ID を採番
    /// - 許容誤差を考慮した節点統合
    /// </summary>
    public class LineToBeam_Advanced : GH_Component
    {
        public LineToBeam_Advanced()
          : base("LineToBeam(Advanced)", "LineToBeam+",
              "Convert Rhino Lines to FEMur Nodes and BeamElements with automatic node deduplication",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "Input lines (Rhino Line)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "Mat", "FEMur Material", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam", GH_ParamAccess.item);
            pManager.AddNumberParameter("BetaAngle", "β", "Local coordinate system rotation angle (degrees, default=0)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("NodeTolerance", "Tol", "Node merging tolerance (default=1e-6)", GH_ParamAccess.item, 1e-6);

            // Optional parameters
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "FEMur Nodes (deduplicated)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "FEMur BeamElements", GH_ParamAccess.list);
            pManager.AddTextParameter("Statistics", "Info", "Model statistics", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var rhinoLines = new List<Line>();
            FEMMat material = null;
            FEMCS crossSection = null;
            double betaAngle = 0.0;
            double nodeTolerance = 1e-6;

            if (!DA.GetDataList(0, rhinoLines)) return;
            if (!DA.GetData(1, ref material)) return;
            if (!DA.GetData(2, ref crossSection)) return;
            DA.GetData(3, ref betaAngle);
            DA.GetData(4, ref nodeTolerance);

            // Validation
            if (material == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Material is null.");
                return;
            }
            if (crossSection == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "CrossSection_Beam is null.");
                return;
            }
            if (rhinoLines.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No lines provided.");
                DA.SetDataList(0, new List<FEMNode>());
                DA.SetDataList(1, new List<FEMBeamElement>());
                DA.SetData(2, "No lines");
                return;
            }

            try
            {
                // Create ModelGeometryBuilder
                var builder = new ModelGeometryBuilder(nodeTolerance);

                // Convert Rhino Lines to GeometryLines
                foreach (var rhinoLine in rhinoLines)
                {
                    // Convert Rhino.Geometry.Point3d to FEMur.Geometry.Point3
                    var startPoint = new FEMPoint3(rhinoLine.From.X, rhinoLine.From.Y, rhinoLine.From.Z);
                    var endPoint = new FEMPoint3(rhinoLine.To.X, rhinoLine.To.Y, rhinoLine.To.Z);

                    var geometryLine = new GeometryLine(startPoint, endPoint)
                    {
                        Material = material,
                        CrossSection = crossSection,
                        BetaAngle = betaAngle
                    };

                    builder.AddLine(geometryLine);
                }

                // Build nodes and elements
                var nodes = builder.GetNodes();
                var elements = builder.BuildBeamElements();

                // Get statistics
                var stats = builder.GetStatistics();

                // Output
                DA.SetDataList(0, nodes);
                DA.SetDataList(1, elements);
                DA.SetData(2, stats);

                // Success message
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, 
                    $"Successfully created {nodes.Count} nodes and {elements.Count} elements");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error: {ex.Message}");
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-4789-A0B1-C2D3E4F5A6B7");
    }
}
