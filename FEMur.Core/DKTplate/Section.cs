using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Interface;

namespace FEMur.Core.DKTplate
{
    public class Section:ISection
    {
        public double Thickness { get; }
        public Section(double thickness)
        {
            this.Thickness = thickness;
        }
        public override string ToString()
        {
            return $"thickness {Thickness}";
        }
    }
}
