using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.FEMur2D.Model
{
    public class Section
    {
        public double thickness { get; }
        public Section(double thickness)
        {
            this.thickness = thickness;
        }

        public override string ToString()
        {
            return $"thickness {thickness}";
        }
    }
}
