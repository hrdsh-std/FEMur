using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Model
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
