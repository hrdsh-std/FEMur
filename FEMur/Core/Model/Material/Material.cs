using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Model.Material
{
    internal class Material
    {
        int id { get; } = 0;
        string name { get; } = string.Empty;
        double E { get; } = 0;
        double nu { get; } = 0;

        public Material(int id,string name , double E , double nu ) { 
            
            this.id = id;
            this.name = name;
            this.E = E;
            this.nu = nu;

        }

        public override string ToString()
        {
            return $"Material {this.id}: Name {this.name}, E {this.E}, NU {this.nu}";
        }

    }
}
