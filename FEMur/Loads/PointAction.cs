using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Nodes;
using FEMur.Geometry;

namespace FEMur.Loads
{
    public abstract class PointAction : Load
    {
        public int NodeId { get; set; } // setterを追加（自動設定のため）
        public Point3? Position { get; protected set; } // Point3による指定を保持

        protected PointAction() { }

        // Point3ベースコンストラクタ（Grasshopper用）
        protected PointAction(Point3 position)
        {
            NodeId = -1; // 未割り当て
            Position = position;
        }

        // NodeIDベースコンストラクタ（既存）
        protected PointAction(int nodeId)
        {
            NodeId = nodeId;
        }

        // Nodeベースコンストラクタ（既存）
        protected PointAction(Node node)
        {
            NodeId = node.Id;
        }
    }
}
