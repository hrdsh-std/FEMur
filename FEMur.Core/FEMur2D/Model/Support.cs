using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.FEMur2D.Model
{
    public class Support
    {
        public Node node { get; }
        public bool DX { get; }
        public bool DY { get; }
        public bool DZ { get; }

        public Support(Node node, bool DX, bool DY, bool DZ)
        {
            this.node = node;
            this.DX = DX;
            this.DY = DY;
            this.DZ = DZ;
        }
    }
}
