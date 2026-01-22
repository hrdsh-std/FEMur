using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using FEMur.Elements;
using FEMur.CrossSections;
using FEMur.Materials;
using FEMur.Geometry;

namespace FEMurGH.Comoponents.Elements
{
    public class BeamElement : GH_Component
    {
        public BeamElement()
            : base("BeamElement", "be",
                "Convert polylines to beam elements for structural analysis (using Point3)",
                "FEMur", "4.Element")
        {
        }

        public override Guid ComponentGuid => new Guid("A0D518A7-9DC8-4FDA-801F-E7F2DAD3926F");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "PL", "Polylines or lines to convert to beam elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Material for beams", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "Cross section for beams (CrossSection_Beam)", GH_ParamAccess.item);
            pManager.AddNumberParameter("BetaAngle", "β", "Local coordinate system rotation angle (degrees)", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BeamElements", "BE", "Output beam elements", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Beam endpoint positions organized by element (DataTree)", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curves = new List<Curve>();
            Material material = null;
            CrossSection_Beam crossSection = null;
            double betaAngle = 0.0;

            if (!DA.GetDataList(0, curves)) return;
            if (!DA.GetData(1, ref material)) return;
            if (!DA.GetData(2, ref crossSection)) return;
            DA.GetData(3, ref betaAngle);

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

            // Convert curves to line segments
            var lineSegments = new List<Line>();
            foreach (var curve in curves)
            {
                if (curve.TryGetPolyline(out Polyline polyline))
                {
                    // ポリラインの各セグメントを取得
                    for (int i = 0; i < polyline.SegmentCount; i++)
                    {
                        lineSegments.Add(polyline.SegmentAt(i));
                    }
                }
                else if (curve.IsLinear())
                {
                    // 直線の場合
                    lineSegments.Add(new Line(curve.PointAtStart, curve.PointAtEnd));
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
                        "Non-linear curves are approximated as lines from start to end points");
                    lineSegments.Add(new Line(curve.PointAtStart, curve.PointAtEnd));
                }
            }

            // Create beam elements using Point3 (IDなし)
            var beams = new List<FEMur.Elements.BeamElement>();
            var pointsTree = new GH_Structure<GH_Point>();
            int zeroLengthCount = 0;
            const double minLength = 1e-6; // 最小要素長

            int elementIndex = 0;
            foreach (var line in lineSegments)
            {
                // Check for zero-length elements
                if (line.Length < minLength)
                {
                    zeroLengthCount++;
                    continue;
                }

                // Rhino Point3d を FEMur Point3 に変換
                var point1 = new Point3(line.From.X, line.From.Y, line.From.Z);
                var point2 = new Point3(line.To.X, line.To.Y, line.To.Z);

                // IDを指定せずにBeamElementを作成（Model内で自動採番される）
                var beam = new FEMur.Elements.BeamElement(point1, point2, material, crossSection, betaAngle);
                beams.Add(beam);

                // Tree構造で各要素ごとに節点を追加
                var path = new GH_Path(elementIndex);
                pointsTree.Append(new GH_Point(line.From), path);
                pointsTree.Append(new GH_Point(line.To), path);

                elementIndex++;
            }

            if (zeroLengthCount > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
                    $"Skipped {zeroLengthCount} zero-length segments (< {minLength:E3})");
            }

            DA.SetDataList(0, beams);
            DA.SetDataTree(1, pointsTree);
        }

        protected override System.Drawing.Bitmap Icon => null;
    }
}