using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.CrossSections;

namespace FEMurGH.Comoponents.Models
{
    /// <summary>
    /// AssembleModel - 断面形状生成機能
    /// </summary>
    public partial class AssembleModel
    {
        #region Cross Section Generation

        /// <summary>
        /// 断面形状の3Dモデルを生成
        /// </summary>
        private void GenerateCrossSectionBreps(List<ElementBase> elements, List<Node> nodes)
        {
            _crossSectionBreps.Clear();
            var nodeById = nodes.ToDictionary(n => n.Id, n => n);

            foreach (var elem in elements)
            {
                if (!(elem is LineElement) || elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                var crossSection = elem.CrossSection as CrossSection_Beam;
                if (crossSection == null)
                    continue;

                if (!nodeById.TryGetValue(elem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(elem.NodeIds[1], out Node n1)) continue;

                var p0 = new Point3d(n0.Position.X, n0.Position.Y, n0.Position.Z);
                var p1 = new Point3d(n1.Position.X, n1.Position.Y, n1.Position.Z);

                if (!elem.TryGetLocalCoordinateSystem(out double[] ex, out double[] ey, out double[] ez))
                {
                    elem.CalcLocalAxis(new List<Node> { n0, n1 });
                    if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                        continue;
                }

                Curve sectionCurve = CreateCrossSectionCurve(crossSection, p0, ey, ez);
                if (sectionCurve == null)
                    continue;

                var rail = new LineCurve(p0, p1);
                var sweepBreps = Brep.CreateFromSweep(rail, sectionCurve, false, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                
                if (sweepBreps != null && sweepBreps.Length > 0)
                {
                    foreach (var brep in sweepBreps)
                    {
                        if (brep != null && brep.IsValid)
                        {
                            _crossSectionBreps.Add(brep);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 断面形状の曲線を生成
        /// </summary>
        private Curve CreateCrossSectionCurve(CrossSection_Beam cs, Point3d origin, double[] localY, double[] localZ)
        {
            Vector3d yVec = new Vector3d(localY[0], localY[1], localY[2]);
            Vector3d zVec = new Vector3d(localZ[0], localZ[1], localZ[2]);

            if (cs is CrossSection_Box boxSection)
                return CreateBoxSectionCurve(origin, yVec, zVec, boxSection);
            else if (cs is CrossSection_H hSection)
                return CreateHSectionCurve(origin, yVec, zVec, hSection);
            else if (cs is CrossSection_Circle circle)
                return CreateCircleCurve(origin, yVec, zVec, circle);

            return null;
        }

        #endregion

        #region Circle Section

        private Curve CreateCircleCurve(Point3d origin, Vector3d yVec, Vector3d zVec, CrossSection_Circle circle)
        {
            var xVec = Vector3d.CrossProduct(yVec, zVec);
            var plane = new Plane(origin, xVec);

            double outerRadius = circle.D / 2.0;
            double innerRadius = (circle.D - 2 * circle.t) / 2.0;

            var outerCircle = new Circle(plane, outerRadius);
            var outerCurve = outerCircle.ToNurbsCurve();

            if (innerRadius > 0 && innerRadius < outerRadius)
            {
                var innerCircle = new Circle(plane, innerRadius);
                var innerCurve = innerCircle.ToNurbsCurve();

                var curves = new List<Curve> { outerCurve, innerCurve };
                var joinedCurves = Curve.JoinCurves(curves, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true);

                if (joinedCurves != null && joinedCurves.Length > 0)
                    return joinedCurves[0];
            }

            return outerCurve;
        }

        #endregion

        #region Box Section

        private Curve CreateBoxSectionCurve(Point3d origin, Vector3d yVec, Vector3d zVec, CrossSection_Box box)
        {
            double B = box.B;
            double H = box.H;
            double t = box.t;

            double halfB = B / 2.0;
            double halfH = H / 2.0;

            var outerP1 = origin - zVec * halfH - yVec * halfB;
            var outerP2 = origin - zVec * halfH + yVec * halfB;
            var outerP3 = origin + zVec * halfH + yVec * halfB;
            var outerP4 = origin + zVec * halfH - yVec * halfB;

            var outerPolyline = new Polyline(new[] { outerP1, outerP2, outerP3, outerP4, outerP1 });
            var outerCurve = outerPolyline.ToNurbsCurve();

            double innerHalfB = halfB - t;
            double innerHalfH = halfH - t;

            if (innerHalfB > 0 && innerHalfH > 0)
            {
                var innerP1 = origin - zVec * innerHalfH - yVec * innerHalfB;
                var innerP2 = origin - zVec * innerHalfH + yVec * innerHalfB;
                var innerP3 = origin + zVec * innerHalfH + yVec * innerHalfB;
                var innerP4 = origin + zVec * innerHalfH - yVec * innerHalfB;

                var innerPolyline = new Polyline(new[] { innerP1, innerP2, innerP3, innerP4, innerP1 });
                var innerCurve = innerPolyline.ToNurbsCurve();

                var curves = new List<Curve> { outerCurve, innerCurve };
                var joinedCurves = Curve.JoinCurves(curves, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true);

                if (joinedCurves != null && joinedCurves.Length > 0)
                    return joinedCurves[0];
            }

            return outerCurve;
        }

        #endregion

        #region H Section

        private Curve CreateHSectionCurve(Point3d origin, Vector3d yVec, Vector3d zVec, CrossSection_H h)
        {
            double H = h.H;
            double B = h.B;
            double tw = h.t_w;
            double tf = h.t_f;

            double halfH = H / 2.0;
            double halfB = B / 2.0;
            double halfTw = tw / 2.0;
            double innerY = halfH - tf;

            var points = new List<Point3d>
            {
                origin - zVec * halfH - yVec * halfB,
                origin - zVec * halfH + yVec * halfB,
                origin - zVec * innerY + yVec * halfB,
                origin - zVec * innerY + yVec * halfTw,
                origin + zVec * innerY + yVec * halfTw,
                origin + zVec * innerY + yVec * halfB,
                origin + zVec * halfH + yVec * halfB,
                origin + zVec * halfH - yVec * halfB,
                origin + zVec * innerY - yVec * halfB,
                origin + zVec * innerY - yVec * halfTw,
                origin - zVec * innerY - yVec * halfTw,
                origin - zVec * innerY - yVec * halfB
            };

            points.Add(points[0]);

            var polyline = new Polyline(points);
            return polyline.ToNurbsCurve();
        }

        #endregion
    }
}