using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using FEMur.Materials;

namespace FEMur.CrossSections
{
    //<summary>
    // 線要素の断面の基底クラス
    // 各数値を直接設定するコンストラクタを用意する
    //</summary>

    public abstract class CrossSection_Beam : CrossSection, ISerializable
    {
        public double A { get;  set; } 
        public double J { get;  set; }
        public double Iyy { get; set; }
        public double Izz { get; set; }
        public double iy { get; set; }
        public double iz { get; set; }

        public  CrossSection_Beam()
        {
        }
        public CrossSection_Beam(int id, string name)
            : base(id, name)
        {
        }
        public CrossSection_Beam(int id, string name, double a, double J,double Iyy, double Izz,double iy, double iz)
            : base(id, name)
        {
            A = a;
            this.J = J;
            this.Iyy = Iyy;
            this.Izz = Izz;
            this.iy = iy;
            this.iz = iz;
        }
        protected CrossSection_Beam(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            A = info.GetDouble("A");
            J = info.GetDouble("J");
            Iyy = info.GetDouble("Iyy");
            Izz = info.GetDouble("Izz");
            iy = info.GetDouble("iy");
            iz = info.GetDouble("iz");
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
        }
    }
}
