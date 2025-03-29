using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Interface
{
    public interface IMaterial
    {
        public double YoungModulus { get; }
        public double PoissonRatio { get; }
        public double massDensity { get; }
        string ToString();
    }
}
