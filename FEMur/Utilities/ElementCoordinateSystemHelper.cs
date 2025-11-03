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
        /// モデル内の全LineElement要素に対して要素座標系を設定する
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
                SetupElementCoordinateSystem(element, model);
            }
        }

        /// <summary>
        /// 個別要素の座標系を設定する
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

            // BetaAngleが設定されている場合は、LineElementのCalcLocalAxisメソッドに委譲
            if (Math.Abs(element.BetaAngle) > 1e-10)
            {
                element.CalcLocalAxis(model.Nodes);
                return;
            }

            // BetaAngleが設定されていない場合は、従来のロジックで計算
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
                // n = v2 - (v2?v1)/(v1?v1) * v1
                // ここで v1 = elementX, v2 = globalZ
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
                // i端またはj端の節点を共有する他の要素を検索
                var connectedElement = FindConnectedElement(element, model, iNode.Id, jNode.Id);

                if (connectedElement != null)
                {
                    // 接続要素が見つかった場合、3点で平面を定義
                    elementZ = CalculateElementZFromConnectedElement(
                        iNode, jNode, connectedElement, model, elementX);
                }
                else
                {
                    // 接続要素がない場合は全体X方向を要素z座標とする
                    elementZ = new double[] { 1, 0, 0 };
                }
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
        /// 指定ノードを共有する他の要素を検索
        /// </summary>
        private static LineElement FindConnectedElement(
            LineElement targetElement, Model model, int iNodeId, int jNodeId)
        {
            return model.Elements
                .OfType<LineElement>()
                .Where(e => e != targetElement && e.NodeIds != null && e.NodeIds.Count >= 2)
                .FirstOrDefault(e => 
                    e.NodeIds.Contains(iNodeId) || e.NodeIds.Contains(jNodeId));
        }

        /// <summary>
        /// 接続要素から要素Z軸を計算
        /// </summary>
        private static double[] CalculateElementZFromConnectedElement(
            Node iNode, Node jNode, LineElement connectedElement, Model model, double[] elementX)
        {
            // 接続要素の他端ノードを取得
            int? otherNodeId = null;
            
            if (connectedElement.NodeIds.Contains(iNode.Id))
            {
                otherNodeId = connectedElement.NodeIds.FirstOrDefault(id => id != iNode.Id);
            }
            else if (connectedElement.NodeIds.Contains(jNode.Id))
            {
                otherNodeId = connectedElement.NodeIds.FirstOrDefault(id => id != jNode.Id);
            }

            if (!otherNodeId.HasValue)
                return new double[] { 1, 0, 0 }; // フォールバック

            var otherNode = model.Nodes.FirstOrDefault(n => n.Id == otherNodeId.Value);
            if (otherNode == null)
                return new double[] { 1, 0, 0 }; // フォールバック

            // 3点で平面を定義
            // i-j-otherの順で平面の法線ベクトルを計算
            var vec1 = new double[3];
            vec1[0] = jNode.Position.X - iNode.Position.X;
            vec1[1] = jNode.Position.Y - iNode.Position.Y;
            vec1[2] = jNode.Position.Z - iNode.Position.Z;

            var vec2 = new double[3];
            vec2[0] = otherNode.Position.X - iNode.Position.X;
            vec2[1] = otherNode.Position.Y - iNode.Position.Y;
            vec2[2] = otherNode.Position.Z - iNode.Position.Z;

            // 平面の法線ベクトル（vec1 × vec2）
            var normal = CrossProduct(vec1, vec2);
            Normalize(normal);

            // 要素Z軸 = 法線 × 要素X軸
            var elementZ = CrossProduct(normal, elementX);
            Normalize(elementZ);

            return elementZ;
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