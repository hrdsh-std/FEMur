using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using FEMur.Utilities;
using FEMur.Nodes;
using FEMur.Geometry;

namespace FEMur.Supports
{
    public class Support: CommonObject, ICloneable,ISerializable
    {
        public int NodeId { get; set; } // setterを追加（自動設定のため）
        public Point3? Position { get; private set; } // Point3による指定を保持
        public bool[] Conditions { get; private set; } = new bool[6];
        public double[] Displacement { get; private set; } = new double[6];
        public double[] Stiffness { get; private set; } = new double[6];
        private Node.DOF[] dofs = new Node.DOF[6];

        public Support() { }

        // Point3ベースコンストラクタ（Grasshopper用）
        public Support(Point3 position, bool fixDX, bool fixDY, bool fixDZ, bool fixRX, bool fixRY, bool fixRZ)
        {
            NodeId = -1; // 未割り当て
            Position = position;
            Conditions = new bool[6] { fixDX, fixDY, fixDZ, fixRX, fixRY, fixRZ };
        }

        public Support(Point3 position, bool[] conditions)
        {
            NodeId = -1; // 未割り当て
            Position = position;
            Conditions = conditions;
        }

        public Support(Point3 position, double dx, double dy, double dz, double rx, double ry, double rz)
        {
            NodeId = -1; // 未割り当て
            Position = position;
            Displacement = new double[6] { dx, dy, dz, rx, ry, rz };
        }

        // NodeIDベースコンストラクタ（既存）
        public Support(int nodeId, double dx, double dy, double dz, double rx, double ry, double rz)
        {
            NodeId = nodeId;
            Displacement = new double[6] { dx, dy, dz, rx, ry, rz };
        }

        public Support(int nodeID, bool fixDX, bool fixDY, bool fixDZ, bool fixRX, bool fixRY, bool fixRZ)
        {
            NodeId = nodeID;
            Conditions = new bool[6] { fixDX, fixDY, fixDZ, fixRX, fixRY, fixRZ };
        }

        public Support(int nodeId, bool[] conditions)
        {
            NodeId = nodeId;
            Conditions = conditions;
        }

        // Nodeベースコンストラクタ（既存）
        public Support(Node node, double dx, double dy, double dz, double rx, double ry, double rz)
        {
            NodeId = node.Id;
            Displacement = new double[6] { dx, dy, dz, rx, ry, rz };
        }

        public Support(Node node, bool fixDX, bool fixDY, bool fixDZ, bool fixRX, bool fixRY, bool fixRZ)
        {
            NodeId = node.Id;
            Conditions = new bool[6] { fixDX, fixDY, fixDZ, fixRX, fixRY, fixRZ };
        }

        public Support(Node node, bool[] conditions)
        {
            NodeId = node.Id;
            Conditions = conditions;
        }
        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        public Support(Support other)
        {
            // 値型フィールド
            this.NodeId = other.NodeId;
            
            // Nullable値型（Point3?）
            this.Position = other.Position;  // Point3はstructなので値がコピーされる
            
            // 配列のディープコピー
            this.Conditions = other.Conditions != null ? (bool[])other.Conditions.Clone() : new bool[6];
            this.Displacement = other.Displacement != null ? (double[])other.Displacement.Clone() : new double[6];
            this.Stiffness = other.Stiffness != null ? (double[])other.Stiffness.Clone() : new double[6];
            this.dofs = other.dofs != null ? (Node.DOF[])other.dofs.Clone() : new Node.DOF[6];
        }

        public override object DeepCopy()
        {
            return new Support(this);
        }

        public override string ToString()
        {
            // 位置情報
            string location;
            if (Position.HasValue)
            {
                location = $"Position=({Position.Value.X:F2}, {Position.Value.Y:F2}, {Position.Value.Z:F2})";
            }
            else
            {
                location = $"NodeId={NodeId}";
            }

            // 支持条件を文字列化 (UX, UY, UZ, RX, RY, RZ)
            string[] conditionNames = new string[6];
            for (int i = 0; i < 6; i++)
            {
                conditionNames[i] = Conditions[i] ? "Fix" : "Free";
            }
            string conditionsStr = $"({string.Join(", ", conditionNames)})";

            return $"Support at {location}, Conditions={conditionsStr}";
        }
    }
}
