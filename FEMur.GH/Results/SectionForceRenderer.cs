using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Geometry;
using FEMur.Models;
using FEMur.Results;
using Rhino.Display;
using Rhino.Geometry;

namespace FEMurGH.Results
{
    internal static class SectionForceRenderer
    {
        public static SectionForcePreview BuildPreview(
            Model model,
            double scale,
            SectionForceView.SectionForceType forceType,
            bool showFilled,
            bool showNumbers)
        {
            var preview = new SectionForcePreview();

            if (model == null || model.Result == null || model.Result.ElementStresses == null)
                return SectionForcePreview.Empty;

            if (forceType == SectionForceView.SectionForceType.None)
                return SectionForcePreview.Empty;

            // 検索用辞書
            var nodeById = model.Nodes.ToDictionary(n => n.Id, n => n);
            var elemById = model.Elements.ToDictionary(e => e.Id, e => e);

            // 表示色（種類ごとに固定色）
            Color color = ForceColor(forceType);

            foreach (var es in model.Result.ElementStresses)
            {
                ElementBase elem;
                if (!elemById.TryGetValue(es.ElementId, out elem))
                    continue;

                var lineElem = elem as LineElement;
                if (lineElem == null || lineElem.NodeIds == null || lineElem.NodeIds.Count < 2)
                    continue;

                Node n0, n1;
                if (!nodeById.TryGetValue(lineElem.NodeIds[0], out n0)) continue;
                if (!nodeById.TryGetValue(lineElem.NodeIds[1], out n1)) continue;

                var p0 = ToRhinoPoint(n0.Position);
                var p1 = ToRhinoPoint(n1.Position);

                // 要素座標系の近傍直交方向（図式の高さ方向）を決める
                Vector3d ex = p1 - p0;
                if (!ex.Unitize() || ex.IsTiny())
                    continue;

                Vector3d up = Vector3d.ZAxis;
                if (Math.Abs(Vector3d.Multiply(ex, up)) > 0.99) up = Vector3d.YAxis; // ほぼ平行なら別軸
                Vector3d ey = Vector3d.CrossProduct(up, ex);
                if (!ey.Unitize() || ey.IsTiny())
                {
                    up = Vector3d.XAxis;
                    ey = Vector3d.CrossProduct(up, ex);
                    if (!ey.Unitize() || ey.IsTiny()) continue;
                }

                // i/j端の値
                double vi, vj;
                if (!TryPickForce(forceType, es, out vi, out vj))
                    continue;

                // 図式スケール
                double hi = vi * scale;
                double hj = vj * scale;

                // 図式ポリライン（基準線からey方向へオフセット）
                var poly = new Polyline(2);
                poly.Add(p0 + ey * hi);
                poly.Add(p1 + ey * hj);

                preview.Diagrams.Add(new DiagramLine
                {
                    Polyline = poly,
                    Color = color,
                    Thickness = 2
                });

                if (showFilled)
                {
                    // 基準線(要素線)と図式の間を塗りつぶし
                    var m = BuildQuadMesh(p0, p1, p0 + ey * hi, p1 + ey * hj, color);
                    preview.FilledMeshes.Add(m);
                }

                if (showNumbers)
                {
                    // 端部の数値ラベル
                    var offSmall = 0.03; // わずかに離す
                    var p0lbl = p0 + ey * (hi + offSmall * ex.Length);
                    var p1lbl = p1 + ey * (hj + offSmall * ex.Length);

                    preview.Labels.Add(new ForceLabel
                    {
                        Location = p0lbl,
                        Text = FormatValue(vi),
                        TextColor = Color.Black,
                        BgColor = Color.White
                    });
                    preview.Labels.Add(new ForceLabel
                    {
                        Location = p1lbl,
                        Text = FormatValue(vj),
                        TextColor = Color.Black,
                        BgColor = Color.White
                    });
                }
            }

            return preview;
        }

        private static string FormatValue(double v)
        {
            // 少数2桁、指数が大きい場合は簡易表記
            if (Math.Abs(v) >= 1e6 || (Math.Abs(v) > 0 && Math.Abs(v) < 1e-2))
                return v.ToString("0.###E+0");
            return v.ToString("0.##");
        }

        private static Color ForceColor(SectionForceView.SectionForceType type)
        {
            switch (type)
            {
                case SectionForceView.SectionForceType.Fx: return Color.OrangeRed;
                case SectionForceView.SectionForceType.Fy: return Color.DodgerBlue;
                case SectionForceView.SectionForceType.Fz: return Color.MediumSeaGreen;
                case SectionForceView.SectionForceType.Mx: return Color.MediumPurple;
                case SectionForceView.SectionForceType.My: return Color.Crimson;
                case SectionForceView.SectionForceType.Mz: return Color.DarkOrange;
                default: return Color.Gray;
            }
        }

        private static bool TryPickForce(SectionForceView.SectionForceType type, ElementStress es, out double vi, out double vj)
        {
            vi = vj = 0.0;
            switch (type)
            {
                case SectionForceView.SectionForceType.Fx:
                    vi = es.Fx_i; vj = es.Fx_j; return true;
                case SectionForceView.SectionForceType.Fy:
                    vi = es.Fy_i; vj = es.Fy_j; return true;
                case SectionForceView.SectionForceType.Fz:
                    vi = es.Fz_i; vj = es.Fz_j; return true;
                case SectionForceView.SectionForceType.Mx:
                    vi = es.Mx_i; vj = es.Mx_j; return true;
                case SectionForceView.SectionForceType.My:
                    vi = es.My_i; vj = es.My_j; return true;
                case SectionForceView.SectionForceType.Mz:
                    vi = es.Mz_i; vj = es.Mz_j; return true;
                default:
                    return false;
            }
        }

        private static Point3d ToRhinoPoint(Point3 p)
        {
            return new Point3d(p.X, p.Y, p.Z);
        }

        private static FilledDiagram BuildQuadMesh(Point3d b0, Point3d b1, Point3d t0, Point3d t1, Color color)
        {
            var mesh = new Mesh();
            int i0 = mesh.Vertices.Add(b0);
            int i1 = mesh.Vertices.Add(b1);
            int i2 = mesh.Vertices.Add(t1);
            int i3 = mesh.Vertices.Add(t0);

            mesh.Faces.AddFace(i0, i1, i2, i3);
            mesh.Normals.ComputeNormals();
            mesh.Compact();

            var mat = new DisplayMaterial(color, 0.6); // 透過少し
            return new FilledDiagram
            {
                Mesh = mesh,
                Material = mat,
                Outline = new Polyline(new[] { b0, b1, t1, t0, b0 }),
                OutlineColor = Color.FromArgb(160, color)
            };
        }
    }

    internal class SectionForcePreview
    {
        public List<DiagramLine> Diagrams { get; } = new List<DiagramLine>();
        public List<FilledDiagram> FilledMeshes { get; } = new List<FilledDiagram>();
        public List<ForceLabel> Labels { get; } = new List<ForceLabel>();

        public static SectionForcePreview Empty => new SectionForcePreview();
    }

    internal class DiagramLine
    {
        public Polyline Polyline { get; set; }
        public Color Color { get; set; }
        public int Thickness { get; set; }
    }

    internal class FilledDiagram
    {
        public Mesh Mesh { get; set; }
        public DisplayMaterial Material { get; set; }
        public Polyline Outline { get; set; }
        public Color OutlineColor { get; set; }
    }

    internal class ForceLabel
    {
        public Point3d Location { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public Color BgColor { get; set; }
    }
}