using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Model.Section
{
    internal class Section
    {
        int id { get; } = 0;
        double thickness { get; } = double.NaN;
        public Section(int id , double thickness) { 
            this.id = id;  
            this.thickness = thickness;
        }

        public override string ToString()
        {
            return $"Section {this.id}: thickness {this.thickness}";
        }
    }
}
