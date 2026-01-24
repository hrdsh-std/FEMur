using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Nodes;
using FEMur.Geometry;

namespace FEMur.Loads
{
    public class PointLoad : PointAction
    {
        public Vector3 Force { get; set; }
        public Vector3 Moment { get; set; }

        public PointLoad() { }

        // Point3ベースコンストラクタ（Grasshopper用）
        public PointLoad(Point3 position, Vector3 force)
            : base(position)
        {
            Force = force;
            Moment = new Vector3(0, 0, 0);
        }

        public PointLoad(Point3 position, Vector3 force, Vector3 moment)
            : base(position)
        {
            Force = force;
            Moment = moment;
        }

        // NodeIDベースコンストラクタ（既存）
        public PointLoad(int nodeId, Vector3 force)
            : base(nodeId)
        {
            Force = force;
            Moment = new Vector3(0, 0, 0);
        }

        public PointLoad(int nodeId, Vector3 force, Vector3 moment)
            : base(nodeId)
        {
            Force = force;
            Moment = moment;
        }

        // Nodeベースコンストラクタ（既存）
        public PointLoad(Node node, Vector3 force)
            : base(node)
        {
            Force = force;
            Moment = new Vector3(0, 0, 0);
        }

        public PointLoad(Node node, Vector3 force, Vector3 moment)
            : base(node)
        {
            Force = force;
            Moment = moment;
        }

        public override string ToString()
        {
            string location;
            if (Position.HasValue)
            {
                location = $"Position=({Position.Value.X:F2}, {Position.Value.Y:F2}, {Position.Value.Z:F2})";
            }
            else
            {
                location = $"NodeId={NodeId}";
            }

            string forceStr = $"Force=({Force.X:F2}, {Force.Y:F2}, {Force.Z:F2})";
            string momentStr = $"Moment=({Moment.X:F2}, {Moment.Y:F2}, {Moment.Z:F2})";

            return $"PointLoad at {location}, {forceStr}, {momentStr}";
        }
    }
}
