using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Interface;

namespace FEMur.Core.DKTplate
{
    public class NodalLoad:ILoad
    {
        public int nodeID { get; }
        public double Fx { get; }
        public double Fy { get; }
        public double Fz { get; }
        public double Mx { get; }
        public double My { get; }
        public double Mz { get; }

        public NodalLoad(int nodeID, double Fx=0.0, double Fy = 0.0, double Fz = 0.0, double Mx = 0.0, double My = 0.0, double Mz = 0.0)
        {
            this.nodeID = nodeID;
            this.Fx = Fx;
            this.Fy = Fy;
            this.Fz = Fz;
            this.Mx = Mx;
            this.My = My;
            this.Mz = Mz;
        }

        public override string ToString()
        {
            return $"Load: NodeID {nodeID} Fx: {Fx}, Fy: {Fy}, Fz: {Fz}, Mx: {Mx}, My: {My}, Mz: {Mz}";
        }

    }
}
