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

namespace FEMurGH.Comoponents.Results
{
    internal static class SectionForceRenderer
    {
        public static SectionForcePreview BuildPreview(
            Model model,
            double scale,
            SectionForceView.SectionForceType forceType,
            bool showFilled,
            bool showNumbers,
            SectionForceView component) // componentを追加
        {
            var preview = new SectionForcePreview();

            if (model == null || model.Result == null || model.Result.ElementStresses == null)
                return SectionForcePreview.Empty;

            if (forceType == SectionForceView.SectionForceType.None)
                return SectionForcePreview.Empty;

            // 断面力タイプがモーメントかどうかを判定
            bool isMoment = (forceType == SectionForceView.SectionForceType.Mx ||
                            forceType == SectionForceView.SectionForceType.My ||
                            forceType == SectionForceView.SectionForceType.Mz);

            // 色スケールのための最小値・最大値を取得
            double minValue = 0.0;
            double maxValue = 0.0;
            GetMinMaxForceValue(model, forceType, out minValue, out maxValue);

            var nodeById = model.Nodes.ToDictionary(n => n.Id, n => n);
            var elemById = model.Elements.ToDictionary(e => e.Id, e => e);

            foreach (var stress in model.Result.ElementStresses)
            {
                var elem = model.Elements.FirstOrDefault(e => e.Id == stress.ElementId);
                if (elem == null || elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                var node_i = model.Nodes.FirstOrDefault(n => n.Id == elem.NodeIds[0]);
                var node_j = model.Nodes.FirstOrDefault(n => n.Id == elem.NodeIds[1]);
                if (node_i == null || node_j == null)
                    continue;

                // 断面力の値を取得（内部単位系: N, N·mm）
                double forceValue_i = 0.0;
                double forceValue_j = 0.0;

                switch (forceType)
                {
                    case SectionForceView.SectionForceType.Fx:
                        forceValue_i = stress.Fx_i;
                        forceValue_j = stress.Fx_j;
                        break;
                    case SectionForceView.SectionForceType.Fy:
                        forceValue_i = stress.Fy_i;
                        forceValue_j = stress.Fy_j;
                        break;
                    case SectionForceView.SectionForceType.Fz:
                        forceValue_i = stress.Fz_i;
                        forceValue_j = stress.Fz_j;
                        break;
                    case SectionForceView.SectionForceType.Mx:
                        forceValue_i = stress.Mx_i;
                        forceValue_j = stress.Mx_j;
                        break;
                    case SectionForceView.SectionForceType.My:
                        forceValue_i = stress.My_i;
                        forceValue_j = stress.My_j;
                        break;
                    case SectionForceView.SectionForceType.Mz:
                        forceValue_i = stress.Mz_i;
                        forceValue_j = stress.Mz_j;
                        break;
                }

                // 要素の座標
                Point3d pi = new Point3d(node_i.Position.X, node_i.Position.Y, node_i.Position.Z);
                Point3d pj = new Point3d(node_j.Position.X, node_j.Position.Y, node_j.Position.Z);

                // 局所座標系を取得
                double[] ex, ey, ez;
                if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                    continue;

                // 断面力タイプに応じてオフセット方向を決定
                Vector3d offsetDir;
                switch (forceType)
                {
                    case SectionForceView.SectionForceType.Fx:  // 軸力 → Z軸方向
                        offsetDir = new Vector3d(ez[0], ez[1], ez[2]);
                        break;
                    case SectionForceView.SectionForceType.Fy:  // せん断力Y → Y軸方向
                        offsetDir = new Vector3d(ey[0], ey[1], ey[2]);
                        break;
                    case SectionForceView.SectionForceType.Fz:  // せん断力Z → Z軸方向
                        offsetDir = new Vector3d(ez[0], ez[1], ez[2]);
                        break;
                    case SectionForceView.SectionForceType.Mx:  // ねじりモーメント → Z軸方向
                        offsetDir = new Vector3d(ez[0], ez[1], ez[2]);
                        break;
                    case SectionForceView.SectionForceType.My:  // 曲げモーメントY → Z軸方向
                        offsetDir = new Vector3d(ez[0], ez[1], ez[2]);
                        break;
                    case SectionForceView.SectionForceType.Mz:  // 曲げモーメントZ → Y軸方向
                        offsetDir = new Vector3d(ey[0], ey[1], ey[2]);
                        break;
                    default:
                        offsetDir = new Vector3d(ey[0], ey[1], ey[2]);
                        break;
                }

                // スケールを適用してオフセット点を計算
                Point3d pi_offset = pi - offsetDir * (forceValue_i * scale);
                Point3d pj_offset = pj - offsetDir * (forceValue_j * scale);

                // メッシュを生成
                if (showFilled)
                {
                    var filledDiagram = CreateFilledDiagram(
                        pi, pi_offset, pj_offset, pj, 
                        forceValue_i, forceValue_j,
                        minValue, maxValue);
                    preview.FilledMeshes.Add(filledDiagram);
                }

                if (showNumbers)
                {
                    // 単位変換を適用
                    double displayValue_i = component.ConvertSectionForceValue(forceValue_i, isMoment);
                    double displayValue_j = component.ConvertSectionForceValue(forceValue_j, isMoment);

                    // 要素軸方向のベクトル
                    Vector3d axisDir = pj - pi;
                    axisDir.Unitize();

                    // 要素長さの10%程度内側にオフセット
                    double elementLength = pi.DistanceTo(pj);
                    double inwardOffset = elementLength * 0.10;
                    
                    // テキストのサイズを考慮したオフセット（概算）
                    // フォントサイズから推定される文字列の幅
                    string text_i = FormatValue(displayValue_i);
                    string text_j = FormatValue(displayValue_j);
                    double charWidth = 6.0; // 1文字あたりの幅（概算、ピクセル単位をmm換算）
                    
                    // i端: 文字の左下を指定（要素軸方向にテキスト幅の分だけオフセット）
                    var p0lbl = pi_offset + axisDir * inwardOffset - offsetDir * 0.03;
                    
                    // j端: 文字の右下を指定（要素軸方向にテキスト幅分戻す）
                    double textWidth_j = text_j.Length * charWidth;
                    var p1lbl = pj_offset - axisDir * (inwardOffset + textWidth_j) - offsetDir * 0.03;

                    preview.Labels.Add(new LabelDot
                    {
                        Location = p0lbl,
                        Text = text_i,
                        TextColor = Color.Black,
                        BgColor = Color.Transparent
                    });
                    preview.Labels.Add(new LabelDot
                    {
                        Location = p1lbl,
                        Text = text_j,
                        TextColor = Color.Black,
                        BgColor = Color.Transparent
                    });
                }
            }

            return preview;
        }

        private static string FormatValue(double v)
        {
            // 小数第1位まで表示、指数表示なし
            return v.ToString("F2");
        }

        private static Point3d ToRhinoPoint(Point3 p)
        {
            return new Point3d(p.X, p.Y, p.Z);
        }

        /// <summary>
        /// 選択された断面力タイプの最小値と最大値を取得
        /// </summary>
        private static void GetMinMaxForceValue(Model model, SectionForceView.SectionForceType forceType, out double minValue, out double maxValue)
        {
            minValue = 0.0;
            maxValue = 0.0;

            if (model.Result == null || model.Result.ElementStresses == null)
                return;

            bool firstValue = true;

            foreach (var stress in model.Result.ElementStresses)
            {
                double value_i = 0.0;
                double value_j = 0.0;

                switch (forceType)
                {
                    case SectionForceView.SectionForceType.Fx:
                        value_i = stress.Fx_i;
                        value_j = stress.Fx_j;
                        break;
                    case SectionForceView.SectionForceType.Fy:
                        value_i = stress.Fy_i;
                        value_j = stress.Fy_j;
                        break;
                    case SectionForceView.SectionForceType.Fz:
                        value_i = stress.Fz_i;
                        value_j = stress.Fz_j;
                        break;
                    case SectionForceView.SectionForceType.Mx:
                        value_i = stress.Mx_i;
                        value_j = stress.Mx_j;
                        break;
                    case SectionForceView.SectionForceType.My:
                        value_i = stress.My_i;
                        value_j = stress.My_j;
                        break;
                    case SectionForceView.SectionForceType.Mz:
                        value_i = stress.Mz_i;
                        value_j = stress.Mz_j;
                        break;
                    case SectionForceView.SectionForceType.None:
                    default:
                        continue;
                }

                if (firstValue)
                {
                    minValue = Math.Min(value_i, value_j);
                    maxValue = Math.Max(value_i, value_j);
                    firstValue = false;
                }
                else
                {
                    minValue = Math.Min(minValue, Math.Min(value_i, value_j));
                    maxValue = Math.Max(maxValue, Math.Max(value_i, value_j));
                }
            }
        }

        /// <summary>
        /// 値を色に変換（コンターマッピング）
        /// </summary>
        /// <param name="value">断面力の値（符号付き）</param>
        /// <param name="minValue">断面力の最小値</param>
        /// <param name="maxValue">断面力の最大値</param>
        /// <returns>コンター色</returns>
        private static Color GetColor(double value, double minValue, double maxValue)
        {
            double range = maxValue - minValue;
            
            // レンジが非常に小さい場合は単色（緑）で表示
            // 最大絶対値の1%未満の変動は無視
            double maxAbs = Math.Max(Math.Abs(minValue), Math.Abs(maxValue));
            double threshold = maxAbs * 0.01; // 1%の閾値
            
            // 絶対値が非常に小さい場合も考慮（例：1e-6未満）
            if (maxAbs < 1e-6)
            {
                threshold = 1e-6;
            }
            
            if (range < threshold)
            {
                // ほぼ同じ値の場合は中間色（緑）で表示
                return Color.FromArgb(255, 0, 255, 0);
            }
            
            // 通常のカラーマッピング
            double t = (value - minValue) / range;
            t = Math.Max(0.0, Math.Min(1.0, t));

            double r = 0, g = 0, b = 0;

            if (t < 0.25)
            {
                // Blue -> Cyan
                r = 0;
                g = 4 * t;
                b = 1;
            }
            else if (t < 0.5)
            {
                // Cyan -> Green
                r = 0;
                g = 1;
                b = 1 + 4 * (0.25 - t);
            }
            else if (t < 0.75)
            {
                // Green -> Yellow
                r = 4 * (t - 0.5);
                g = 1;
                b = 0;
            }
            else
            {
                // Yellow -> Red
                r = 1;
                g = 1 + 4 * (0.75 - t);
                b = 0;
            }

            return Color.FromArgb(
                255,
                (int)(255 * r),
                (int)(255 * g),
                (int)(255 * b)
            );
        }

        /// <summary>
        /// 断面力図のメッシュを作成（直接Meshで生成）
        /// </summary>
        /// <param name="pi">i端の軸上の点</param>
        /// <param name="pi_off">i端のオフセット点</param>
        /// <param name="pj_off">j端のオフセット点</param>
        /// <param name="pj">j端の軸上の点</param>
        /// <param name="value_i">i端の断面力値</param>
        /// <param name="value_j">j端の断面力値</param>
        /// <param name="minValue">全体の最小値（色計算用）</param>
        /// <param name="maxValue">全体の最大値（色計算用）</param>
        /// <returns>FilledDiagram</returns>
        private static FilledDiagram CreateFilledDiagram(
            Point3d pi, Point3d pi_off, Point3d pj_off, Point3d pj,
            double value_i, double value_j,
            double minValue, double maxValue)
        {
            /////<要修正> 線形応力にしか対応していない処理なので、梁用素荷重実装時に見直しが必要

            // メッシュを作成
            Mesh mesh = new Mesh();

            //構造芯のラインを作成
            Line line0 = new Line(pi, pj);
            Line line1 = new Line(pi_off,pj_off);

            var intersections = Rhino.Geometry.Intersect.Intersection.LineLine(line0, line1, out double a, out double b);

            List<double> line0Params = new List<double>();
            List<double> line1Params = new List<double>();

            //交点のパラメータを保存
            if (intersections)
            {
                line0Params.Add(a);
                line1Params.Add(b);
            }
            //端点のパラメータを保存
            line0Params.Add(0.0);
            line0Params.Add(1.0);
            line1Params.Add(0.0);
            line1Params.Add(1.0);

            //パラメータでソート
            line0Params.Sort();
            line1Params.Sort();

            //重複削除
            line0Params = line0Params.Distinct().ToList();
            line1Params = line1Params.Distinct().ToList();

            List<Curve> boundary = new List<Curve>();

            List<Brep> regions = new List<Brep>();

            var outline = new Polyline();

            if (intersections && a > 0 && a < 1)
            {
                Point3d p00 = line0.PointAt(line0Params[0]);
                Point3d p01 = line0.PointAt(line0Params[1]);
                Point3d p02 = line0.PointAt(line0Params[2]);
                Point3d p10 = line1.PointAt(line1Params[0]);
                Point3d p12 = line1.PointAt(line1Params[2]);

                mesh.Vertices.Add(p00);     // 0
                mesh.Vertices.Add(p01);     // 1
                mesh.Vertices.Add(p02);     // 2
                mesh.Vertices.Add(p10);     // 3
                mesh.Vertices.Add(p12);     // 4

                //面を追加
                mesh.Faces.AddFace(0,1,3);
                mesh.Faces.AddFace(1,4,2);

                //各頂点に色を設定（アルファ値を指定して半透明に）
                int alpha = 180; // 透明度 0-255（180 = 約70%の不透明度）
                Color color_p00 = Color.FromArgb(alpha, GetColor(value_i, minValue, maxValue));
                Color color_p01 = Color.FromArgb(alpha, GetColor(0.0, minValue, maxValue));
                Color color_p02 = Color.FromArgb(alpha, GetColor(value_j, minValue, maxValue));
                Color color_p10 = Color.FromArgb(alpha, GetColor(value_i, minValue, maxValue));
                Color color_p12 = Color.FromArgb(alpha, GetColor(value_j, minValue, maxValue));

                mesh.VertexColors.Add(color_p00);
                mesh.VertexColors.Add(color_p01);
                mesh.VertexColors.Add(color_p02);
                mesh.VertexColors.Add(color_p10);
                mesh.VertexColors.Add(color_p12);

                //法線を計算
                mesh.Normals.ComputeNormals();
                //輪郭線
                outline = new Polyline(new[] { p10, p00, p01, p02 , p12, p10 });
            }
            else
            {
                mesh.Vertices.Add(pi);       // 0: i端軸上
                mesh.Vertices.Add(pi_off);   // 1: i端オフセット
                mesh.Vertices.Add(pj_off);   // 2: j端オフセット
                mesh.Vertices.Add(pj);       // 3: j端軸上

                mesh.Faces.AddFace(0, 1, 2);
                mesh.Faces.AddFace(0, 2, 3);

                int alpha = 180; // 透明度 0-255
                Color color_pi = Color.FromArgb(alpha, GetColor(value_i, minValue, maxValue));
                Color color_pi_off = Color.FromArgb(alpha, GetColor(value_i, minValue, maxValue));
                Color color_pj_off = Color.FromArgb(alpha, GetColor(value_j, minValue, maxValue));
                Color color_pj = Color.FromArgb(alpha, GetColor(value_j, minValue, maxValue));

                mesh.VertexColors.Add(color_pi);
                mesh.VertexColors.Add(color_pi_off);
                mesh.VertexColors.Add(color_pj_off);
                mesh.VertexColors.Add(color_pj);

                //法線を計算
                mesh.Normals.ComputeNormals();

                //輪郭線
                outline = new Polyline(new[] { pi, pi_off, pj_off, pj, pi });
            }

            // DisplayMaterialを半透明に設定
            var material = new DisplayMaterial(Color.White);
            material.Transparency = 0.7; // 透明度 0.0(不透明) - 1.0(完全透明)

            return new FilledDiagram
            {
                Mesh = mesh,
                Material = material,
                Outline = outline,
                OutlineColor = Color.Black,
                ValueAtI = value_i,
                ValueAtJ = value_j
            };
        }
    }
}