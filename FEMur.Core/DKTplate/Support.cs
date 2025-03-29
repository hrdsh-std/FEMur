using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Interface;

namespace FEMur.Core.DKTplate
{
    public class Support:ISupport
    {
        public int nodeID { get; }
        public bool DX { get; }
        public bool DY { get; }
        public bool DZ { get; }
        public bool RX { get; }
        public bool RY { get; }
        public bool RZ { get; }

        public Support(int nodeID, bool DX=false, bool DY = false, bool DZ = false, bool RX = false, bool RY = false, bool RZ = false)
        {
            this.nodeID = nodeID;
            this.DX = DX;
            this.DY = DY;
            this.DZ = DZ;
            this.RX = RX;
            this.RY = RY;
            this.RZ = RZ;
        }

        public override string ToString()
        {
            //固定、自由を01で{000000}の形式で表現 true=1 false=0
            return $"Support Node {nodeID}: {Convert.ToInt32(DX)}{Convert.ToInt32(DY)}{Convert.ToInt32(DZ)}{Convert.ToInt32(RX)}{Convert.ToInt32(RY)}{Convert.ToInt32(RZ)}";

        }

    }
}
