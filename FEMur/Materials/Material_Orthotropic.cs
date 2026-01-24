using System;
using System.Runtime.Serialization;

namespace FEMur.Materials
{
    public class Material_Orthotropic : Material
    {
        public double Ex { get; private set; }
        public double Ey { get; private set; }
        public double Ez { get; private set; }
        public double Gxy { get; private set; }
        public double Gyz { get; private set; }
        public double Gzx { get; private set; }
        public double Nuxy { get; private set; }
        public double Nuyz { get; private set; }
        public double Nuzx { get; private set; }

        public Material_Orthotropic() { }

        public Material_Orthotropic(
            string family,
            string name,
            double density,
            double ex, double ey, double ez,
            double gxy, double gyz, double gzx,
            double nuxy, double nuyz, double nuzx)
            : base(family, name, ex, nuxy, density)
        {
            Ex = ex;
            Ey = ey;
            Ez = ez;
            Gxy = gxy;
            Gyz = gyz;
            Gzx = gzx;
            Nuxy = nuxy;
            Nuyz = nuyz;
            Nuzx = nuzx;
        }

        public Material_Orthotropic(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            return $"Material_Orthotropic: {Name}, Ex={Ex:F0}, Ey={Ey:F0}, Ez={Ez:F0} N/mm^2";
        }
    }
}
