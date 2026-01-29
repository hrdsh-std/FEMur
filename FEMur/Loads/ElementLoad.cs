using System;
using System.Runtime.Serialization;
using FEMur.Geometry;
using FEMur.Elements;
using FEMur.Nodes;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Loads
{
    // 線要素に作用する等分布荷重(ローカル座標系)を表現
    public class ElementLoad : Load, ISerializable
    {
        // 対象要素ID
        public int ElementId { get; set; }

        // ローカル座標系の等分布荷重 [N/mm](x, y, z)
        public Vector3 QLocal { get; set; }

        // ローカル座標系の等分布ねじり [N*mm/mm](mx など)。未使用なら(0,0,0)
        public Vector3 MLocal { get; set; }

        // ローカル荷重かどうか(現状 true のみを想定)
        public bool Local { get; set; } = true;

        public ElementLoad() { }

        public ElementLoad(int elementId, Vector3 qLocal)
        {
            ElementId = elementId;
            QLocal = qLocal;
            MLocal = new Vector3(0.0, 0.0, 0.0);
            Local = true;
        }

        public ElementLoad(int elementId, Vector3 qLocal, Vector3 mLocal, bool local = true)
        {
            ElementId = elementId;
            QLocal = qLocal;
            MLocal = mLocal;
            Local = local;
        }

        // 追加: Element を直接渡せるコンストラクタ
        public ElementLoad(ElementBase element, Vector3 qLocal)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            ElementId = element.Id;
            QLocal = qLocal;
            MLocal = new Vector3(0.0, 0.0, 0.0);
            Local = true;
        }

        // 追加: Element を直接渡せるコンストラクタ (モーメント付き)
        public ElementLoad(ElementBase element, Vector3 qLocal, Vector3 mLocal, bool local = true)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            ElementId = element.Id;
            QLocal = qLocal;
            MLocal = mLocal;
            Local = local;
        }

        protected ElementLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ElementId = info.GetInt32("ElementId");
            QLocal = (Vector3)info.GetValue("QLocal", typeof(Vector3));
            MLocal = (Vector3)info.GetValue("MLocal", typeof(Vector3));
            Local = info.GetBoolean("Local");
        }
        public ElementLoad(ElementLoad other):base(other)
        {
            ElementId = other.ElementId;
            QLocal = other.QLocal;
            MLocal = other.MLocal;
            Local = other.Local;
        }

        public override object DeepCopy()
        {
            return new ElementLoad(this);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ElementId", ElementId);
            info.AddValue("QLocal", QLocal);
            info.AddValue("MLocal", MLocal);
            info.AddValue("Local", Local);
        }

        // 一様分布荷重の等価節点荷重(ローカル座標系 12x1)を返す
        // DOF順: [uX1,uY1,uZ1,rX1,rY1,rZ1, uX2,uY2,uZ2,rX2,rY2,rZ2]
        public Vector<double> CalcEquivalentNodalLoadLocal(ElementBase element, System.Collections.Generic.List<Node> nodes)
        {
            // 要素長 L
            var n1 = FindNodeById(nodes, element.NodeIds[0]);
            var n2 = FindNodeById(nodes, element.NodeIds[1]);
            double dx = n2.Position.X - n1.Position.X;
            double dy = n2.Position.Y - n1.Position.Y;
            double dz = n2.Position.Z - n1.Position.Z;
            double L = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

            double qx = QLocal.X;
            double qy = QLocal.Y;
            double qz = QLocal.Z;

            double mx = MLocal.X; // ねじり分布

            var fe = Vector<double>.Build.Dense(12, 0.0);

            // 軸方向一様分布 qx -> 端節点に qx*L/2 ずつ
            if (System.Math.Abs(qx) > 0.0)
            {
                fe[0] += qx * L / 2.0;
                fe[6] += qx * L / 2.0;
            }

            // 横方向一様分布 qy(ローカル+Y) -> Fy, Mz
            if (System.Math.Abs(qy) > 0.0)
            {
                fe[1] += qy * L / 2.0;                 // Fy1
                fe[7] += qy * L / 2.0;                 // Fy2
                fe[5] += qy * L * L / 12.0;            // Mz1
                fe[11] += -qy * L * L / 12.0;          // Mz2
            }

            // 横方向一様分布 qz(ローカル+Z) -> Fz, My
            if (System.Math.Abs(qz) > 0.0)
            {
                fe[2] += qz * L / 2.0;                 // Fz1
                fe[8] += qz * L / 2.0;                 // Fz2
                fe[4] += -qz * L * L / 12.0;           // My1
                fe[10] += qz * L * L / 12.0;           // My2
            }

            // ねじり一様分布 mx(ローカル+X) -> Mx
            if (System.Math.Abs(mx) > 0.0)
            {
                fe[3] += mx * L / 2.0;                 // Mx1
                fe[9] += mx * L / 2.0;                 // Mx2
            }

            return fe;
        }

        private static Node FindNodeById(System.Collections.Generic.List<Node> nodes, int id)
        {
            // ノードIDとインデックスの一致を前提にしない
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Id == id) return nodes[i];
            }
            throw new System.Exception($"Node with Id={id} not found.");
        }
    }
}
