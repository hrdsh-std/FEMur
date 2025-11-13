using System;

namespace FEMur.Geometry.Intermediate
{
    /// <summary>
    /// 中間表現：位置情報のみを持つノード（ID不要）
    /// RhinoGeometryからFEMur構造解析モデルへの変換を容易にする
    /// </summary>
    public class GeometryNode : IEquatable<GeometryNode>
    {
        /// <summary>
        /// ノードの位置座標
        /// </summary>
        public Point3 Position { get; }

        /// <summary>
        /// オプショナルな参照情報（元のRhinoオブジェクトのGUIDなど）
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// コンストラクタ：Point3から生成
        /// </summary>
        public GeometryNode(Point3 position)
        {
            Position = position;
        }

        /// <summary>
        /// コンストラクタ：座標値から生成
        /// </summary>
        public GeometryNode(double x, double y, double z)
        {
            Position = new Point3(x, y, z);
        }

        /// <summary>
        /// FEMur.Nodes.Nodeへ変換（IDを指定）
        /// </summary>
        public Nodes.Node ToNode(int id)
        {
            return new Nodes.Node(id, Position);
        }

        /// <summary>
        /// 等価性チェック（位置座標が一致するか）
        /// </summary>
        public bool Equals(GeometryNode other)
        {
            if (other == null) return false;
            return Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GeometryNode);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override string ToString()
        {
            return $"GeometryNode({Position.X:F3}, {Position.Y:F3}, {Position.Z:F3})";
        }
    }
}
