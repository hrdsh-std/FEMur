using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Model
{


    public class Load
    {
        public Node node { get; }
        public double Fx { get; }
        public double Fy { get; }
        public double Fz { get; }
        public Load(Node node, double Fx, double Fy, double Fz=0)
        {
            this.node = node;
            this.Fx = Fx;
            this.Fy = Fy;
            this.Fz = Fz;
        }

    }
}
