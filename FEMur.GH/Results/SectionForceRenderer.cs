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

            var nodeById = model.Nodes.ToDictionary(n => n.Id, n => n);
            var elemById = model.Elements.ToDictionary(e => e.Id, e => e);

            Color color = ForceColor(forceType);

            foreach (var es in model.Result.ElementStresses)
            {
                if (!elemById.TryGetValue(es.ElementId, out ElementBase elem))
                    continue;

                var lineElem = elem as LineElement;
                if (lineElem == null || lineElem.NodeIds == null || lineElem.NodeIds.Count < 2)
                    continue;

                if (!nodeById.TryGetValue(lineElem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(lineElem.NodeIds[1], out Node n1)) continue;

                var p0 = ToRhinoPoint(n0.Position);
                var p1 = ToRhinoPoint(n1.Position);

                // 局所座標系を取得（要素から）
                double[] ex, ey, ez;
                Vector3d exVec, eyVec, ezVec;
                
                if (elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                {
                    // 要素が局所座標系を保持している場合
                    exVec = new Vector3d(ex[0], ex[1], ex[2]);
                    eyVec = new Vector3d(ey[0], ey[1], ey[2]);
                    ezVec = new Vector3d(ez[0], ez[1], ez[2]);
                }
                else
                {
                    // 局所座標系が未計算の場合は計算
                    lineElem.CalcLocalAxis(new List<Node> { n0, n1 });
                    
                    if (elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                    {
                        exVec = new Vector3d(ex[0], ex[1], ex[2]);
                        eyVec = new Vector3d(ey[0], ey[1], ey[2]);
                        ezVec = new Vector3d(ez[0], ez[1], ez[2]);
                    }
                    else
                    {
                        // フォールバック: 部材軸から局所座標系を構築
                        exVec = p1 - p0;
                        if (!exVec.Unitize() || exVec.IsTiny())
                            continue;

                        Vector3d up = Vector3d.ZAxis;
                        if (Math.Abs(Vector3d.Multiply(exVec, up)) > 0.99) up = Vector3d.YAxis;

                        ezVec = Vector3d.CrossProduct(up, exVec);
                        if (!ezVec.Unitize() || ezVec.IsTiny())
                        {
                            up = Vector3d.XAxis;
                            ezVec = Vector3d.CrossProduct(up, exVec);
                            if (!ezVec.Unitize() || ezVec.IsTiny()) continue;
                        }
                        eyVec = Vector3d.CrossProduct(ezVec, exVec);
                        if (!eyVec.Unitize() || eyVec.IsTiny()) continue;
                    }
                }

                // 応力値 i/j
                if (!TryPickForce(forceType, es, out double vi, out double vj))
                    continue;

                // 図式オフセット方向:
                // 局所Y方向の荷重(Fy, Mz)と軸力(Fx)、ねじりモーメント(Mx)は局所Y方向に描画
                // 局所Z方向の荷重(Fz, My)は局所Z方向に描画
                // i端: 各方向の正方向を正とする
                // j端: 各方向の負方向を正とする（符号を反転）
                Vector3d offsetDir_i, offsetDir_j;
                
                switch (forceType)
                {
                    case SectionForceView.SectionForceType.Fz:
                    case SectionForceView.SectionForceType.My:
                        // 局所Z方向に描画
                        offsetDir_i = ezVec;  // i端: +Z方向が正
                        offsetDir_j = -ezVec; // j端: -Z方向が正
                        break;
                    
                    case SectionForceView.SectionForceType.Fx:
                    case SectionForceView.SectionForceType.Fy:
                    case SectionForceView.SectionForceType.Mx:
                    case SectionForceView.SectionForceType.Mz:
                    default:
                        // 局所Y方向に描画
                        offsetDir_i = eyVec;  // i端: +Y方向が正
                        offsetDir_j = -eyVec; // j端: -Y方向が正
                        break;
                }

                // スケール適用
                double hi = vi * scale;
                double hj = vj * scale;

                // 図式ポリライン（i端とj端で異なるオフセット方向を使用）
                var poly = new Polyline(2);
                poly.Add(p0 + offsetDir_i * hi);
                poly.Add(p1 + offsetDir_j * hj);

                preview.Diagrams.Add(new DiagramLine
                {
                    Polyline = poly,
                    Color = color,
                    Thickness = 2
                });

                if (showFilled)
                {
                    var m = BuildQuadMesh(p0, p1, p0 + offsetDir_i * hi, p1 + offsetDir_j * hj, color);
                    preview.FilledMeshes.Add(m);
                }

                if (showNumbers)
                {
                    // 少し法線方向に離して重なり回避
                    var offSmall = 0.03;
                    var p0lbl = p0 + offsetDir_i * (hi + offSmall);
                    var p1lbl = p1 + offsetDir_j * (hj + offSmall);

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

            var mat = new DisplayMaterial(color, 0.6);
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