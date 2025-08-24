using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using FEMur.Materials;

namespace FEMur.CrossSections
{
    public abstract class CrossSection_Beam : CrossSection, ISerializable
    {
        public double A { get; private set; } 
        public double Ay { get; private set; }
        public double Az { get; private set; }
        public double Cw { get; private set; }
        public double J { get; private set; }
        public double Iyy { get; private set; }
        public double Izz { get; private set; }
        public double iy { get; private set; }
        public double iz { get; private set; }
        private Material Material { get; set; }

        public string MaterialName => this.Material.Name;
        protected CrossSection_Beam()
        {
        }
        protected CrossSection_Beam(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            A = info.GetDouble("A");
            Ay = info.GetDouble("Ay");
            Az = info.GetDouble("Az");
            Cw = info.GetDouble("Cw");
            J = info.GetDouble("J");
            Iyy = info.GetDouble("Iyy");
            Izz = info.GetDouble("Izz");
            iy = info.GetDouble("iy");
            iz = info.GetDouble("iz");
            Material = (Material)info.GetValue("Material", typeof(Material));
        }

        public void Calculate_iy_iz()
        {
            iy = Math.Sqrt(Iyy / A);
            iz = Math.Sqrt(Izz / A);
        }
        public void SetMaterial(Material material = null)
        {
            if(material == null)
            {
                //materialがNULLの場合は鉄をデフォルトとする
                throw new Exception("If material is null, material must be set to steel.But it is not implemented yet.");
            }
            Material = material;
        }
        public void Stiffness(double L, out double EA, out double EIyy, out double EIzz, out double GJ)
        {
            if (Material == null)
            {
                throw new Exception("Material is not set. Please set a material before calculating stiffness.");
            }
            double E = Material.E;
            EA = Material.Density * A / L;
            EIyy = Material.Density * Iyy / L;
            EIzz = Material.Density * Izz / L;
            GJ = Material.Density * J / L;
        }
    }
}
