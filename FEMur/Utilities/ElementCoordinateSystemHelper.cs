using System;
using System.Collections.Generic;
using System.Linq;
using FEMur.Models;
using FEMur.Elements;
using FEMur.Nodes;
using FEMur.Geometry;

namespace FEMur.Utilities
{
    public static class ElementCoordinateSystemHelper
    {
        /// <summary>
        /// モデル内の全LineElement要素に対して要素座標系を設定する（Model初期化時に必ず実行）
        /// </summary>
        /// <param name="model">対象のモデル</param>
        public static void SetupElementCoordinateSystems(Model model)
        {
            if (model == null || model.Elements == null || model.Nodes == null)
                return;

            // LineElement系の要素のみを対象とする
            var lineElements = model.Elements.OfType<LineElement>().ToList();

            foreach (var element in lineElements)
            {
                // Model初期化時に必ずCalcLocalAxisを実行
                SetupElementCoordinateSystem(element, model);
            }
        }

        /// <summary>
        /// 個別要素の座標系を設定する（必ずCalcLocalAxisを実行）
        /// </summary>
        /// <param name="element">対象要素</param>
        /// <param name="model">モデル</param>
        private static void SetupElementCoordinateSystem(LineElement element, Model model)
        {
            if (element.NodeIds == null || element.NodeIds.Count < 2)
                return;

            // 要素のi端、j端ノードを取得
            var iNode = model.Nodes.FirstOrDefault(n => n.Id == element.NodeIds[0]);
            var jNode = model.Nodes.FirstOrDefault(n => n.Id == element.NodeIds[1]);

            if (iNode == null || jNode == null)
                return;

            // LineElement.CalcLocalAxisメソッドを必ず実行
            // β角の設定有無に関わらず、統一的な計算ロジックを使用
            try
            {
                element.CalcLocalAxis(model.Nodes);
            }
            catch (Exception ex)
            {
                // CalcLocalAxisが失敗した場合は、フォールバック処理
                System.Diagnostics.Debug.WriteLine($"Element {element.Id}: CalcLocalAxis failed - {ex.Message}. Using fallback.");
                SetupElementCoordinateSystemFallback(element, iNode, jNode);
            }
        }

        /// <summary>
        /// CalcLocalAxisが失敗した場合のフォールバック処理
        /// </summary>
        private static void SetupElementCoordinateSystemFallback(LineElement element, Node iNode, Node jNode)
        {
            // 要素x軸方向ベクトル（単位ベクトル）
            var elementX = new double[3];
            elementX[0] = jNode.Position.X - iNode.Position.X;
            elementX[1] = jNode.Position.Y - iNode.Position.Y;
            elementX[2] = jNode.Position.Z - iNode.Position.Z;
            
            double length = Math.Sqrt(elementX[0] * elementX[0] + 
                                     elementX[1] * elementX[1] + 
                                     elementX[2] * elementX[2]);
            
            if (length < 1e-10)
                return; // 長さがゼロの要素は処理しない

            elementX[0] /= length;
            elementX[1] /= length;
            elementX[2] /= length;

            // 全体Z軸ベクトル
            var globalZ = new double[] { 0, 0, 1 };

            // 要素x軸と全体Z軸の平行度をチェック
            double dotProduct = Math.Abs(elementX[0] * globalZ[0] + 
                                        elementX[1] * globalZ[1] + 
                                        elementX[2] * globalZ[2]);

            double[] elementZ;

            if (dotProduct < 0.999) // 要素x軸が全体Zに平行でない場合
            {
                // 要素x軸に垂直で全体Z軸を含む平面内のベクトルを計算
                double dot_v2_v1 = globalZ[0] * elementX[0] + 
                                   globalZ[1] * elementX[1] + 
                                   globalZ[2] * elementX[2];
                
                double dot_v1_v1 = elementX[0] * elementX[0] + 
                                   elementX[1] * elementX[1] + 
                                   elementX[2] * elementX[2];
                
                double scalar = dot_v2_v1 / dot_v1_v1;
                
                elementZ = new double[3];
                elementZ[0] = globalZ[0] - scalar * elementX[0];
                elementZ[1] = globalZ[1] - scalar * elementX[1];
                elementZ[2] = globalZ[2] - scalar * elementX[2];
                
                Normalize(elementZ);
            }
            else // 要素x軸が全体Zに平行な場合
            {
                // 全体X方向を要素z座標とする
                elementZ = new double[] { 1, 0, 0 };
            }

            // 要素y軸を計算（elementY = elementZ × elementX）
            var elementY = CrossProduct(elementZ, elementX);
            Normalize(elementY);

            // 要素座標系を設定
            element.LocalAxisX = elementX;
            element.LocalAxisY = elementY;
            element.LocalAxisZ = elementZ;
        }

        /// <summary>
        /// 外積計算
        /// </summary>
        private static double[] CrossProduct(double[] a, double[] b)
        {
            return new double[]
            {
                a[1] * b[2] - a[2] * b[1],
                a[2] * b[0] - a[0] * b[2],
                a[0] * b[1] - a[1] * b[0]
            };
        }

        /// <summary>
        /// ベクトルの正規化
        /// </summary>
        private static void Normalize(double[] vector)
        {
            double length = Math.Sqrt(vector[0] * vector[0] + 
                                     vector[1] * vector[1] + 
                                     vector[2] * vector[2]);
            
            if (length > 1e-10)
            {
                vector[0] /= length;
                vector[1] /= length;
                vector[2] /= length;
            }
        }
    }
}