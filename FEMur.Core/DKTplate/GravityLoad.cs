using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Interface;

namespace FEMur.Core.DKTplate
{
    public class GravityLoad : ILoad
    {
        public double gx { get; }
        public double gy { get; }
        public double gz { get; }
        public GravityLoad(double gx, double gy, double gz)
        {
            this.gx = gx;
            this.gy = gy;
            this.gz = gz;
        }

        public override string ToString()
        {
            return $"Gravity Load: gx: {gx}, gy: {gy}, gz: {gz}";
        }
    }
}
