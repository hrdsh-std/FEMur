using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Model.Node
{
    internal class Node
    {
        int id { get; } = 0;
        double x { get; } = 0;
        double y { get; } = 0;
        double z { get; } = 0;

        public Node(int id, double x, double y,double z)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"Node {this.id}: x {this.x}, y {this.y}, z {this.z}";
        }


    }
}
