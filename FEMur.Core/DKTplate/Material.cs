using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Interface;

namespace FEMur.Core.DKTplate
{
    public class Material: IMaterial
    {
        public double YoungModulus { get; }
        public double PoissonRatio { get; }
        public double massDensity { get; }

        public Material(double youngModulus, double poissonRatio, double massDensity)
        {
            this.YoungModulus = youngModulus;
            this.PoissonRatio = poissonRatio;
            this.massDensity = massDensity;
        }
        public override string ToString()
        {
            return $"YoungModulus:{YoungModulus} PoissonRatio:{PoissonRatio} MassDensity:{massDensity}";
        }
    }
}
