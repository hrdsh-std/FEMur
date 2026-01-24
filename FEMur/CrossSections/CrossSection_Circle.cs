using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.CrossSections
{
    /// <summary>
    /// 円形断面（中空）クラス - パイプ断面
    /// </summary>
    public class CrossSection_Circle : CrossSection_Beam
    {
        public double D { get; set; }   // 外径 [mm]
        public double t { get; set; }   // 板厚 [mm]

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CrossSection_Circle() { }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">断面名</param>
        /// <param name="d">外径 [mm]</param>
        /// <param name="t">板厚 [mm]（0の場合は中実円形）</param>
        public CrossSection_Circle(string name, double d, double t = 0)
            : base(name)
        {
            D = d;
            this.t = t;
            CalculateSectionProperties();
        }

        /// <summary>
        /// シリアライゼーション用コンストラクタ
        /// </summary>
        protected CrossSection_Circle(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            D = info.GetDouble("D");
            t = info.GetDouble("t");
        }

        /// <summary>
        /// 断面性能を計算
        /// </summary>
        protected void CalculateSectionProperties()
        {
            if (t <= 0 || t >= D / 2.0)
            {
                // 中実円形断面として計算
                CalculateSolidCircularSection();
            }
            else
            {
                // 中空（円形鋼管）として計算
                CalculateHollowCircularSection();
            }
        }

        /// <summary>
        /// 中実円形断面の計算
        /// </summary>
        private void CalculateSolidCircularSection()
        {
            double R = D / 2.0;  // 半径

            // 断面積 [mm²]
            this.A = Math.PI * R * R;

            // 断面二次モーメント [mm⁴]
            // 円形断面はY軸、Z軸周りで対称
            double I = (Math.PI * Math.Pow(D, 4)) / 64.0;
            this.Iyy = I;
            this.Izz = I;

            // 断面二次半径 [mm]
            this.iy = D / 4.0;  // R/2
            this.iz = D / 4.0;

            // ねじり定数 [mm⁴]
            // 中実円形断面: J = Ip (極断面二次モーメント)
            this.J = (Math.PI * Math.Pow(D, 4)) / 32.0;
        }

        /// <summary>
        /// 中空円形鋼管の計算
        /// </summary>
        private void CalculateHollowCircularSection()
        {
            double R_outer = D / 2.0;        // 外半径
            double R_inner = R_outer - t;    // 内半径

            // 断面積 [mm²]
            this.A = Math.PI * (R_outer * R_outer - R_inner * R_inner);

            // 断面二次モーメント [mm⁴]
            double I = (Math.PI / 64.0) * (Math.Pow(D, 4) - Math.Pow(D - 2 * t, 4));
            this.Iyy = I;
            this.Izz = I;

            // 断面二次半径 [mm]
            double i = Math.Sqrt(I / A);
            this.iy = i;
            this.iz = i;

            // ねじり定数 [mm⁴]
            // 円形中空断面: J = Ip (極断面二次モーメント)
            this.J = (Math.PI / 32.0) * (Math.Pow(D, 4) - Math.Pow(D - 2 * t, 4));
        }

        /// <summary>
        /// シリアライゼーション用データ取得
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D", D);
            info.AddValue("t", t);
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            if (t <= 0)
            {
                return $"CrossSection_Circle(Name={Name}(Solid))";
            }
            else
            {
                return $"CrossSection_Circle(Name={Name}(Hollow))";
            }
        }

        /// <summary>
        /// クローン作成
        /// </summary>
        public override object Clone()
        {
            return new CrossSection_Circle(Name, D, t);
        }
    }
}
