using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Rhino.Geometry.SubDDisplayParameters;
using System.Xml.Linq;

namespace FEMur.Materials
{
    public class Material_Orthotropic:Material, ISerializable
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

        public Material_Orthotropic(
            string name,
            double density,
            double ex, double ey, double ez,
            double gxy, double gyz, double gzx,
            double nuxy, double nuyz, double nuzx
            ) : base(name, density)
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

        protected Material_Orthotropic(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Ex = info.GetDouble("Ex");
            Ey = info.GetDouble("Ey");
            Ez = info.GetDouble("Ez");

            Gxy = info.GetDouble("Gxy");
            Gyz = info.GetDouble("Gyz");
            Gzx = info.GetDouble("Gzx");

            Nuxy = info.GetDouble("Nuxy");
            Nuyz = info.GetDouble("Nuyz");
            Nuzx = info.GetDouble("Nuzx");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Density", Density);

            info.AddValue("Ex", Ex);
            info.AddValue("Ey", Ey);
            info.AddValue("Ez", Ez);

            info.AddValue("Gxy", Gxy);
            info.AddValue("Gyz", Gyz);
            info.AddValue("Gzx", Gzx);

            info.AddValue("Nuxy", Nuxy);
            info.AddValue("Nuyz", Nuyz);
            info.AddValue("Nuzx", Nuzx);
        }

        public override string ToString()
        {
            return base.ToString() +
                   $", Ex={Ex}, Ey={Ey}, Ez={Ez}, " +
                   $"Gxy={Gxy}, Gyz={Gyz}, Gzx={Gzx}, " +
                   $"νxy={Nuxy}, νyz={Nuyz}, νzx={Nuzx}";
        }
    }
}
