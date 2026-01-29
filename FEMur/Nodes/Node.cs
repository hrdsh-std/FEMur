using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Geometry;
using FEMur.Utilities;

namespace FEMur.Nodes
{
    public class Node:CommonObject,ISerializable
    {
        public int Id { get; set; } // setterを追加（自動採番のため）
        public Point3 Position { get; }
        public Point3 Position_disp { get; private set; }

        // IDなしコンストラクタ（推奨）
        public Node(Point3 position)
        {
            Id = -1; // 未割り当て
            this.Position = position;
            this.Position_disp = position;
        }

        public Node(double x, double y, double z)
        {
            Id = -1; // 未割り当て
            this.Position = new Point3(x, y, z);
            this.Position_disp = new Point3(x, y, z);
        }

        // ID指定コンストラクタ（既存コードとの互換性のため）
        public Node(int id, Point3 position)
        {
            Id = id;
            this.Position = position;
            this.Position_disp = position;
        }

        public Node(int id, double x, double y, double z)
        {
            Id = id;
            this.Position = new Point3(x, y, z);
            this.Position_disp = new Point3(x, y, z);
        }

        public Node(Node other)
        {
            this.Position = other.Position;
            this.Position_disp = other.Position_disp;
            this.Id = other.Id;
        }

        public enum DOF
        {
            DX,
            DY,
            DZ,
            RX,
            RY, 
            RZ,
            N_DOF
        }
        public override object DeepCopy()
        {
            return new Node(this);
        }

        public override string ToString()
        {
            // 座標を小数第2位までで表示
            return $"node-ind:{this.Id}: ({this.Position.X:F2}/{this.Position.Y:F2}/{this.Position.Z:F2})";
        }
    }
}
