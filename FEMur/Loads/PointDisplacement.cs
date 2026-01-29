using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Nodes;
using FEMur.Geometry;

namespace FEMur.Loads
{
    public class PointDisplacement : PointAction
    {
        public Vector3 Displacement { get; set; }
        public Vector3 Rotation { get; set; }

        public PointDisplacement() { }

        // Point3ベースコンストラクタ（Grasshopper用）
        public PointDisplacement(Point3 position, Vector3 displacement)
            : base(position)
        {
            Displacement = displacement;
            Rotation = new Vector3(0, 0, 0);
        }

        public PointDisplacement(Point3 position, Vector3 displacement, Vector3 rotation)
            : base(position)
        {
            Displacement = displacement;
            Rotation = rotation;
        }

        // NodeIDベースコンストラクタ（既存）
        public PointDisplacement(int nodeId, Vector3 displacement)
            : base(nodeId)
        {
            Displacement = displacement;
            Rotation = new Vector3(0, 0, 0);
        }

        public PointDisplacement(int nodeId, Vector3 displacement, Vector3 rotation)
            : base(nodeId)
        {
            Displacement = displacement;
            Rotation = rotation;
        }

        // Nodeベースコンストラクタ（既存）
        public PointDisplacement(Node node, Vector3 displacement)
            : base(node)
        {
            Displacement = displacement;
            Rotation = new Vector3(0, 0, 0);
        }

        public PointDisplacement(Node node, Vector3 displacement, Vector3 rotation)
            : base(node)
        {
            Displacement = displacement;
            Rotation = rotation;
        }
        public PointDisplacement (PointDisplacement other)
            : base(other)
        {
            Displacement = other.Displacement;
            Rotation = other.Rotation;
        }
        public override object DeepCopy()
        {
            return new PointDisplacement(this);
        }

        public override string ToString()
        {
            if (Position.HasValue)
            {
                return $"PointDisplacement at ({Position.Value.X:F2}, {Position.Value.Y:F2}, {Position.Value.Z:F2}), " +
                       $"Displacement=({Displacement.X:F2}, {Displacement.Y:F2}, {Displacement.Z:F2}), NodeId={NodeId}";
            }
            return $"PointDisplacement at NodeId={NodeId}, Displacement=({Displacement.X:F2}, {Displacement.Y:F2}, {Displacement.Z:F2})";
        }
    }
}
