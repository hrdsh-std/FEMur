using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Materials
{
    public class Material_Isotropic : Material
    {
        public Material_Isotropic() { }

        public Material_Isotropic(string family, string name, double density, double E, double nu)
            : base(family, name, E, nu, density)
        {
        }

        public Material_Isotropic(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static Material_Isotropic Steel()
        {
            return new Material_Isotropic("Isotropic", "Steel", 7.85e-9, 210000.0, 0.3);
        }

        public static Material_Isotropic Aluminum()
        {
            return new Material_Isotropic("Isotropic", "Aluminum", 2.70e-9, 70000.0, 0.33);
        }

        public static Material_Isotropic Concrete()
        {
            return new Material_Isotropic("Isotropic", "Concrete", 2.40e-9, 30000.0, 0.2);
        }

        public override string ToString()
        {
            return $"Material_Isotropic: {Name}, E={E:F0} N/mm^2, nu={Nu:F2}, Rho={Density:E2} t/mm^3";
        }
    }
}
