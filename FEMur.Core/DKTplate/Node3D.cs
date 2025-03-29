using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Common;

namespace FEMur.Core.DKTplate
{
    public class Node3D : INode
    {
        public int ID { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public Node3D(int ID, double X, double Y, double Z)
        {
            this.ID = ID;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public override string ToString()
        {
            return $"ID:{ID} X:{X} Y:{Y} Z:{Z}";
        }
    }
}
