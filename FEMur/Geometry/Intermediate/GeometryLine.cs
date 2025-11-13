using System;
using FEMur.Materials;
using FEMur.CrossSections;

namespace FEMur.Geometry.Intermediate
{
    /// <summary>
    /// 中間表現：2つのジオメトリノードを結ぶ線要素
    /// LineToBeam変換処理をサポート
    /// </summary>
    public class GeometryLine
    {
        /// <summary>
        /// 始点ノード
        /// </summary>
        public GeometryNode StartNode { get; }

        /// <summary>
        /// 終点ノード
        /// </summary>
        public GeometryNode EndNode { get; }

        /// <summary>
        /// オプショナル：材料特性
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// オプショナル：断面特性
        /// </summary>
        public CrossSection_Beam CrossSection { get; set; }

        /// <summary>
        /// オプショナル：β角（度）
        /// </summary>
        public double BetaAngle { get; set; } = 0.0;

        /// <summary>
        /// オプショナルな参照情報（元のRhinoオブジェクトのGUIDなど）
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// コンストラクタ：2つのGeometryNodeから生成
        /// </summary>
        public GeometryLine(GeometryNode startNode, GeometryNode endNode)
        {
            StartNode = startNode ?? throw new ArgumentNullException(nameof(startNode));
            EndNode = endNode ?? throw new ArgumentNullException(nameof(endNode));
        }

        /// <summary>
        /// コンストラクタ：2つのPoint3から生成
        /// </summary>
        public GeometryLine(Point3 startPoint, Point3 endPoint)
        {
            StartNode = new GeometryNode(startPoint);
            EndNode = new GeometryNode(endPoint);
        }

        /// <summary>
        /// 線の長さを計算
        /// </summary>
        public double Length
        {
            get
            {
                var dx = EndNode.Position.X - StartNode.Position.X;
                var dy = EndNode.Position.Y - StartNode.Position.Y;
                var dz = EndNode.Position.Z - StartNode.Position.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }

        /// <summary>
        /// 線の中点を取得
        /// </summary>
        public Point3 MidPoint
        {
            get
            {
                return new Point3(
                    (StartNode.Position.X + EndNode.Position.X) / 2.0,
                    (StartNode.Position.Y + EndNode.Position.Y) / 2.0,
                    (StartNode.Position.Z + EndNode.Position.Z) / 2.0
                );
            }
        }

        /// <summary>
        /// 方向ベクトルを取得（正規化されていない）
        /// </summary>
        public Point3 Direction
        {
            get
            {
                return new Point3(
                    EndNode.Position.X - StartNode.Position.X,
                    EndNode.Position.Y - StartNode.Position.Y,
                    EndNode.Position.Z - StartNode.Position.Z
                );
            }
        }

        public override string ToString()
        {
            return $"GeometryLine(Start: {StartNode}, End: {EndNode}, Length: {Length:F3})";
        }
    }
}
