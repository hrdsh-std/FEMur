using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Interface
{
    public interface ISupport
    {
        public int nodeID{ get; }
        public bool DX { get; }
        public bool DY { get; }
        public bool DZ { get; }
        public bool RX { get; }
        public bool RY { get; }
        public bool RZ { get; }


        string ToString();
    }
}
