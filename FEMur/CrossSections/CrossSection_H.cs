using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.CrossSections
{
    public class CrossSection_H : CrossSection_Beam
    {
        public double B { get; set; } // フランジ幅mm
        public double H { get; set; } // 全高mm
        public double t_f { get; set; } // フランジ厚mm
        public double t_w { get; set; } // ウェブ厚mm
        public double r { get; set; } // 断面の内側のRmm

        public CrossSection_H() { }
        public CrossSection_H(string name, double b, double h, double t_f, double t_w, double r)
            : base(name)
        {
            B = b;
            H = h;
            this.t_f = t_f;
            this.t_w = t_w;
            this.r = r;
            CalculateSectionProperties();
        }
        protected void CalculateSectionProperties()
        {
            // 断面積
            this.A = 2 * B * t_f + (H - 2 * t_f) * t_w;
            // 慣性モーメント
            this.Iyy = (B * Math.Pow(H, 3) / 12) - ((B - t_w) * Math.Pow(H - 2 * t_f, 3) / 12);
            this.Izz = (2 * (t_f * Math.Pow(B, 3) / 12)) + ((H - 2 * t_f) * Math.Pow(t_w, 3) / 12);
            // 偏心距離
            this.iy = Math.Sqrt(Iyy / A);
            this.iz = Math.Sqrt(Izz / A);
            // 偏心モーメント
            this.J = (1 / 3.0) * (2 * B * Math.Pow(t_f, 3) + (H - 2 * t_f) * Math.Pow(t_w, 3));
        }
        public override string ToString()
        {
            return $"CrossSection_H(Name={Name})";
        }

    }
}
