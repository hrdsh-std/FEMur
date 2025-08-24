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
        public int Id { get; }
        public Point3 Position { get; }
        public Point3 Position_disp { get; private set; }

        public Node(int id, Point3 position)
        {
            Id = id;
            this.Position = position;
            this.Position_disp = position;
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

        public override string ToString()
        {
            return $"node-ind:{this.Id.ToString()}: ({this.Position.X.ToString()}/{this.Position.Y.ToString()}/{this.Position.Z.ToString()})";
        }

    }

}
