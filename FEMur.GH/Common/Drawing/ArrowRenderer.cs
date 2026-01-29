using System;
using System.Collections.Generic;
using Rhino.Display;
using Rhino.Geometry;

namespace FEMurGH.Common.Drawing
{
    /// <summary>
    /// 矢印の描画処理を担当するレンダラー
    /// </summary>
    public static class ArrowRenderer
    {
        /// <summary>
        /// 力の矢印を描画
        /// </summary>
        /// <param name="display">描画パイプライン</param>
        /// <param name="arrow">矢印ジオメトリ</param>
        /// <param name="showNumbers">数値を表示するかどうか</param>
        /// <param name="valueText">表示する数値テキスト（単位変換済み）</param>
        /// <param name="fontSize">フォントサイズ</param>
        public static void DrawForceArrow(DisplayPipeline display, ArrowGeometry arrow, 
            bool showNumbers = true, string valueText = null, int fontSize = 12)
        {
            if (display == null || arrow == null)
                return;

            // 矢印の線
            display.DrawArrow(new Line(arrow.Start, arrow.End), arrow.Color, 20, 5);

            // ラベル（数値）
            if (showNumbers && !string.IsNullOrEmpty(valueText))
            {
                display.Draw2dText(valueText, arrow.Color, arrow.Start, true, fontSize);
            }
        }

        /// <summary>
        /// モーメントの矢印を描画（二重矢印、右ネジ系）
        /// </summary>
        /// <param name="display">描画パイプライン</param>
        /// <param name="moment">モーメント矢印ジオメトリ</param>
        /// <param name="showNumbers">数値を表示するかどうか</param>
        /// <param name="valueText">表示する数値テキスト（単位変換済み）</param>
        /// <param name="fontSize">フォントサイズ</param>
        public static void DrawMomentArrow(DisplayPipeline display, MomentArrowGeometry moment, 
            bool showNumbers = true, string valueText = null, int fontSize = 12)
        {
            if (display == null || moment == null)
                return;

            // モーメントの大きさが小さすぎる場合はスキップ
            if (moment.Radius < 0.01)
                return;

            // 回転軸に垂直な平面上に円弧を描画
            Vector3d axis = moment.Axis;
            axis.Unitize();

            // 軸ごとに作業平面を定義（右手系を保証）
            Vector3d perpendicular1, perpendicular2;

            if (Math.Abs(axis.X - 1.0) < 0.01) // X軸周りのモーメント（Mx）
            {
                // X軸周り: Y軸を基準、Z軸方向へ
                perpendicular1 = Vector3d.YAxis;
                perpendicular2 = Vector3d.ZAxis;
            }
            else if (Math.Abs(axis.Y - 1.0) < 0.01) // Y軸周りのモーメント（My）
            {
                // Y軸周り: Z軸を基準、X軸方向へ
                perpendicular1 = Vector3d.ZAxis;
                perpendicular2 = Vector3d.XAxis;
            }
            else if (Math.Abs(axis.Z - 1.0) < 0.01) // Z軸周りのモーメント（Mz）
            {
                // Z軸周り: X軸を基準、Y軸方向へ
                perpendicular1 = Vector3d.XAxis;
                perpendicular2 = Vector3d.YAxis;
            }
            else // 一般的な軸（念のため）
            {
                // フォールバック: 従来のロジック
                if (Math.Abs(axis.X) < 0.9)
                {
                    perpendicular1 = Vector3d.CrossProduct(axis, Vector3d.XAxis);
                }
                else
                {
                    perpendicular1 = Vector3d.CrossProduct(axis, Vector3d.YAxis);
                }
                perpendicular1.Unitize();
                perpendicular2 = Vector3d.CrossProduct(axis, perpendicular1);
                perpendicular2.Unitize();
            }

            // 円弧を描画（右ネジの法則）
            int segments = 32;
            double arcAngle = 1.5 * Math.PI; // 270度の円弧
            double angleStep = arcAngle / segments;

            List<Point3d> arcPoints = new List<Point3d>();

            for (int i = 0; i <= segments; i++)
            {
                double angle = i * angleStep;

                // 右ネジの法則: 
                // 正のモーメント → 軸の正方向を向いて見た時、反時計回り
                // 負のモーメント → 軸の正方向を向いて見た時、時計回り
                // clockwise = true の時、右ネジ
                double actualAngle = moment.Clockwise ? angle : -angle;

                Vector3d radial = perpendicular1 * Math.Cos(actualAngle) + perpendicular2 * Math.Sin(actualAngle);
                Point3d point = moment.Center + radial * moment.Radius;
                arcPoints.Add(point);
            }

            // 外側の円弧を描画
            if (arcPoints.Count > 1)
            {
                for (int i = 0; i < arcPoints.Count - 1; i++)
                {
                    display.DrawLine(arcPoints[i], arcPoints[i + 1], moment.Color, 3);
                }

                // 矢印の先端（終点）
                Point3d arrowTip = arcPoints[arcPoints.Count - 1];
                
                // 接線ベクトルを計算
                if (arcPoints.Count >= 2)
                {
                    Vector3d tangent = arcPoints[arcPoints.Count - 1] - arcPoints[arcPoints.Count - 2];
                    tangent.Unitize();
                    
                    // 矢印ヘッドを描画（接線方向）
                    double arrowSize = moment.Radius * 0.3;
                    
                    // 法線方向（半径方向内向き）
                    Vector3d radialIn = moment.Center - arrowTip;
                    radialIn.Unitize();
                    
                    // 矢印の両側
                    Vector3d arrowLeft = tangent * (-arrowSize) + radialIn * (arrowSize * 0.3);
                    Vector3d arrowRight = tangent * (-arrowSize) - radialIn * (arrowSize * 0.3);

                    display.DrawLine(arrowTip, arrowTip + arrowLeft, moment.Color, 3);
                    display.DrawLine(arrowTip, arrowTip + arrowRight, moment.Color, 3);
                }

                // ラベル（数値を単位変換して表示）
                if (showNumbers && !string.IsNullOrEmpty(valueText))
                {
                    Point3d labelPos = arcPoints[arcPoints.Count / 2]; // 円弧の中央にラベル
                    display.Draw2dText(valueText, moment.Color, labelPos, true, fontSize);
                }
            }
        }
    }
}