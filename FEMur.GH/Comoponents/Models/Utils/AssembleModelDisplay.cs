using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Supports;
using FEMur.Loads;

namespace FEMurGH.Comoponents.Models
{
    /// <summary>
    /// AssembleModel - ビューポート描画機能
    /// </summary>
    public partial class AssembleModel
    {
        #region Generate Display Geometry

        /// <summary>
        /// 要素の線材モデルを生成
        /// </summary>
        private void GenerateElementLines(List<ElementBase> elements, List<Node> nodes)
        {
            _elementLines.Clear();
            var nodeById = nodes.ToDictionary(n => n.Id, n => n);

            foreach (var elem in elements)
            {
                if (!(elem is LineElement) || elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                if (!nodeById.TryGetValue(elem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(elem.NodeIds[1], out Node n1)) continue;

                var p0 = new Point3d(n0.Position.X, n0.Position.Y, n0.Position.Z);
                var p1 = new Point3d(n1.Position.X, n1.Position.Y, n1.Position.Z);

                _elementLines.Add(new Line(p0, p1));
            }
        }

        /// <summary>
        /// 局所座標系の矢印を生成
        /// </summary>
        private void GenerateLocalAxisArrows(List<ElementBase> elements, List<Node> nodes)
        {
            _localAxisArrows.Clear();
            var nodeById = nodes.ToDictionary(n => n.Id, n => n);

            foreach (var elem in elements)
            {
                if (elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                if (!nodeById.TryGetValue(elem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(elem.NodeIds[1], out Node n1)) continue;

                var p0 = new Point3d(n0.Position.X, n0.Position.Y, n0.Position.Z);
                var p1 = new Point3d(n1.Position.X, n1.Position.Y, n1.Position.Z);
                var center = (p0 + p1) * 0.5;

                if (!elem.TryGetLocalCoordinateSystem(out double[] ex, out double[] ey, out double[] ez))
                {
                    elem.CalcLocalAxis(new List<Node> { n0, n1 });
                    if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                        continue;
                }

                double elemLength = p0.DistanceTo(p1);
                double arrowLength = elemLength * LocalAxisScale;

                AddLocalAxisArrow(center, ex, arrowLength, Color.Red, "X");
                AddLocalAxisArrow(center, ey, arrowLength, Color.Green, "Y");
                AddLocalAxisArrow(center, ez, arrowLength, Color.Blue, "Z");
            }
        }

        private void AddLocalAxisArrow(Point3d center, double[] axis, double length, Color color, string label)
        {
            var axisVec = new Vector3d(axis[0], axis[1], axis[2]) * length;
            _localAxisArrows.Add(new LocalAxisArrow
            {
                Start = center,
                End = center + axisVec,
                Color = color,
                Label = label
            });
        }

        #endregion

        #region Viewport Rendering

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cachedModel == null) return;

            var display = args.Display;

            DrawElementLines(display);
            DrawNodeIds(display);
            DrawElementIds(display);
            DrawLoads(display);
            DrawSupports(display, args.Viewport);
            DrawLocalAxisArrows(display);
            DrawCrossSectionWires(display);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (ShowCrossSection && _crossSectionBreps != null)
            {
                DrawCrossSectionMeshes(args.Display);
            }
        }

        private void DrawElementLines(Rhino.Display.DisplayPipeline display)
        {
            if (_elementLines != null && _elementLines.Count > 0)
            {
                foreach (var line in _elementLines)
                {
                    display.DrawLine(line, Color.FromArgb(60, 60, 60), 2);
                }
            }
        }

        private void DrawNodeIds(Rhino.Display.DisplayPipeline display)
        {
            if (ShowNodeId)
            {
                foreach (var kvp in _nodePositions)
                {
                    display.DrawDot(kvp.Value, kvp.Key.ToString(), Color.DarkBlue, Color.White);
                }
            }
        }

        private void DrawElementIds(Rhino.Display.DisplayPipeline display)
        {
            if (ShowElementId)
            {
                foreach (var kvp in _elementCenters)
                {
                    display.DrawDot(kvp.Value, kvp.Key.ToString(), Color.DarkGreen, Color.White);
                }
            }
        }

        private void DrawLoads(Rhino.Display.DisplayPipeline display)
        {
            if (ShowLoad && _cachedModel.Loads != null)
            {
                foreach (var load in _cachedModel.Loads)
                {
                    if (load is PointLoad pointLoad)
                        DrawPointLoad(display, pointLoad);
                    else if (load is ElementLoad elementLoad)
                        DrawElementLoad(display, elementLoad);
                }
            }
        }

        private void DrawSupports(Rhino.Display.DisplayPipeline display, Rhino.Display.RhinoViewport viewport)
        {
            if (ShowSupport && _cachedModel.Supports != null)
            {
                foreach (var support in _cachedModel.Supports)
                {
                    if (_nodePositions.ContainsKey(support.NodeId))
                    {
                        DrawSupportSymbol(display, viewport, _nodePositions[support.NodeId], support.Conditions);
                    }
                }
            }
        }

        private void DrawLocalAxisArrows(Rhino.Display.DisplayPipeline display)
        {
            if (ShowLocalAxis && _localAxisArrows != null)
            {
                foreach (var arrow in _localAxisArrows)
                {
                    var line = new Line(arrow.Start, arrow.End);
                    display.DrawArrow(line, arrow.Color, 15, 10);

                    var labelPos = arrow.End + (arrow.End - arrow.Start) * 0.1;
                    display.DrawDot(labelPos, arrow.Label, arrow.Color, Color.White);
                }
            }
        }

        private void DrawCrossSectionWires(Rhino.Display.DisplayPipeline display)
        {
            if (ShowCrossSection && _crossSectionBreps != null)
            {
                foreach (var brep in _crossSectionBreps)
                {
                    if (brep != null && brep.IsValid)
                    {
                        display.DrawBrepWires(brep, Color.DarkGray, 1);
                    }
                }
            }
        }

        private void DrawCrossSectionMeshes(Rhino.Display.DisplayPipeline display)
        {
            var material = new Rhino.Display.DisplayMaterial(Color.LightGray, 0.5);

            foreach (var brep in _crossSectionBreps)
            {
                if (brep != null && brep.IsValid)
                {
                    display.DrawBrepShaded(brep, material);
                }
            }
        }

        #endregion

        #region Draw Load Methods

        private void DrawPointLoad(Rhino.Display.DisplayPipeline display, PointLoad load)
        {
            if (!_nodePositions.ContainsKey(load.NodeId)) return;

            var pt = _nodePositions[load.NodeId];
            var force = load.Force;
            var moment = load.Moment;

            double fMag = Math.Sqrt(force.X * force.X + force.Y * force.Y + force.Z * force.Z);
            if (fMag > 1e-6)
            {
                double scale = 0.1;
                var forceVec = new Vector3d(force.X, force.Y, force.Z);
                forceVec.Unitize();
                forceVec *= scale;

                var arrowEnd = pt + forceVec;
                display.DrawArrow(new Line(pt, arrowEnd), Color.Red, 10, 5);

                var labelOffset = forceVec * 0.2;
                var labelPt = arrowEnd + labelOffset;
                display.DrawDot(labelPt, $"{fMag:F1}", Color.Red, Color.White);
            }

            double mMag = Math.Sqrt(moment.X * moment.X + moment.Y * moment.Y + moment.Z * moment.Z);
            if (mMag > 1e-6)
            {
                var circle = new Circle(pt, 0.05);
                display.DrawCircle(circle, Color.Orange, 2);
                display.DrawDot(pt + new Vector3d(0, 0, 0.07), $"{mMag:F1}", Color.Orange, Color.White);
            }
        }

        private void DrawElementLoad(Rhino.Display.DisplayPipeline display, ElementLoad load)
        {
            var elem = _cachedModel.Elements.FirstOrDefault(e => e.Id == load.ElementId);
            if (elem == null || elem.NodeIds == null || elem.NodeIds.Count < 2) return;

            var n0Id = elem.NodeIds[0];
            var n1Id = elem.NodeIds[1];
            if (!_nodePositions.ContainsKey(n0Id) || !_nodePositions.ContainsKey(n1Id)) return;

            var p0 = _nodePositions[n0Id];
            var p1 = _nodePositions[n1Id];
            var center = (p0 + p1) * 0.5;

            var q = load.QLocal;
            double qMag = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
            if (qMag > 1e-6)
            {
                double scale = 0.1;
                var qVec = new Vector3d(q.X, q.Y, q.Z);
                qVec.Unitize();
                qVec *= scale;

                var arrowEnd = center + qVec;
                display.DrawArrow(new Line(center, arrowEnd), Color.Magenta, 8, 4);

                var labelOffset = qVec * 0.2;
                var labelPt = arrowEnd + labelOffset;
                display.DrawDot(labelPt, $"{qMag:F1}", Color.Magenta, Color.White);
            }
        }

        #endregion

        #region Draw Support Symbol

        private void DrawSupportSymbol(Rhino.Display.DisplayPipeline display, Rhino.Display.RhinoViewport viewport,
            Point3d worldCenter, bool[] conditions)
        {
            if (conditions == null || conditions.Length < 6) return;

            Color outlineColor = Color.DarkRed;
            Color fillColor = Color.Red;
            int screenRadius = 10;

            var screenCenterPoint = viewport.WorldToClient(worldCenter);
            double screenCenterX = screenCenterPoint.X;
            double screenCenterY = screenCenterPoint.Y;

            Vector3d cameraDir = viewport.CameraDirection;
            Plane viewPlane = new Plane(worldCenter, cameraDir);

            Point3d[] worldHexPoints = new Point3d[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 2 - i * Math.PI / 3;
                int screenX = (int)(screenCenterX + screenRadius * Math.Cos(angle));
                int screenY = (int)(screenCenterY - screenRadius * Math.Sin(angle));

                Line ray = viewport.ClientToWorld(new System.Drawing.Point(screenX, screenY));
                if (Rhino.Geometry.Intersect.Intersection.LinePlane(ray, viewPlane, out double t))
                {
                    worldHexPoints[i] = ray.PointAt(t);
                }
                else
                {
                    worldHexPoints[i] = worldCenter;
                }
            }

            var hexPoly = new Polyline(worldHexPoints);
            hexPoly.Add(worldHexPoints[0]);
            display.DrawPolyline(hexPoly, outlineColor, 2);

            for (int i = 0; i < 6; i++)
            {
                display.DrawLine(worldCenter, worldHexPoints[i], outlineColor, 1);
            }

            for (int i = 0; i < 6; i++)
            {
                if (conditions[i])
                {
                    int next = (i + 1) % 6;
                    var mesh = new Mesh();
                    mesh.Vertices.Add(worldCenter);
                    mesh.Vertices.Add(worldHexPoints[i]);
                    mesh.Vertices.Add(worldHexPoints[next]);
                    mesh.Faces.AddFace(0, 1, 2);
                    mesh.Normals.ComputeNormals();

                    display.DrawMeshShaded(mesh, new Rhino.Display.DisplayMaterial(fillColor));
                }
            }
        }

        #endregion

        #region Clipping Box

        public override BoundingBox ClippingBox
        {
            get
            {
                if (_nodePositions.Count == 0) return BoundingBox.Empty;

                var bbox = BoundingBox.Empty;
                foreach (var pt in _nodePositions.Values)
                {
                    bbox.Union(pt);
                }

                if (ShowLocalAxis && _localAxisArrows != null)
                {
                    foreach (var arrow in _localAxisArrows)
                    {
                        bbox.Union(arrow.Start);
                        bbox.Union(arrow.End);
                    }
                }

                if (ShowCrossSection && _crossSectionBreps != null)
                {
                    foreach (var brep in _crossSectionBreps)
                    {
                        if (brep != null && brep.IsValid)
                        {
                            bbox.Union(brep.GetBoundingBox(false));
                        }
                    }
                }

                bbox.Inflate(1.0);
                return bbox;
            }
        }

        #endregion
    }
}
