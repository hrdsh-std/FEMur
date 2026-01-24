using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.CrossSections
{
    /// <summary>
    /// 角形鋼管（Box）の断面クラス
    /// t=0の場合は中実矩形断面として扱う
    /// </summary>
    public class CrossSection_Box : CrossSection_Beam
    {
        public double B { get; set; }  // 幅（Z軸方向）[mm]
        public double H { get; set; }  // 高さ（Y軸方向）[mm]
        public double t { get; set; }  // 板厚 [mm] (0の場合は中実)
        public double r { get; set; }  // コーナー部の内側R [mm]

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CrossSection_Box() { }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="name">断面名</param>
        /// <param name="b">幅（Z軸方向）[mm]</param>
        /// <param name="h">高さ（Y軸方向）[mm]</param>
        /// <param name="t">板厚 [mm]（0の場合は中実矩形）</param>
        /// <param name="r">コーナー部の内側R [mm]（省略時は板厚の1.5倍、中実の場合は0）</param>
        public CrossSection_Box(string name, double b, double h, double t = 0, double r = -1)
            : base(name)
        {
            B = b;
            H = h;
            this.t = t;
            
            // r のデフォルト値設定
            if (t > 0)
            {
                // 中空の場合: デフォルトは板厚の1.5倍
                this.r = r < 0 ? t * 1.5 : r;
            }
            else
            {
                // 中実の場合: r は意味がないので0
                this.r = 0;
            }
            
            CalculateSectionProperties();
        }

        /// <summary>
        /// シリアライゼーション用コンストラクタ
        /// </summary>
        protected CrossSection_Box(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            B = info.GetDouble("B");
            H = info.GetDouble("H");
            t = info.GetDouble("t");
            r = info.GetDouble("r");
        }

        /// <summary>
        /// 断面性能を計算
        /// </summary>
        protected void CalculateSectionProperties()
        {
            if (t <= 0 || t >= Math.Min(B, H) / 2.0)
            {
                // 中実矩形断面として計算
                CalculateSolidRectangularSection();
            }
            else
            {
                // 中空（角形鋼管）として計算
                CalculateHollowBoxSection();
            }
        }

        /// <summary>
        /// 中実矩形断面の計算
        /// </summary>
        private void CalculateSolidRectangularSection()
        {
            // 断面積 [mm²]
            this.A = B * H;

            // 断面二次モーメント [mm⁴]
            // Iyy: Y軸周りの慣性モーメント（曲げ軸がZ軸）
            this.Iyy = (B * Math.Pow(H, 3)) / 12.0;

            // Izz: Z軸周りの慣性モーメント（曲げ軸がY軸）
            this.Izz = (H * Math.Pow(B, 3)) / 12.0;

            // 断面二次半径 [mm]
            this.iy = Math.Sqrt(Iyy / A);
            this.iz = Math.Sqrt(Izz / A);

            // ねじり定数 [mm⁴]
            // 矩形断面のねじり定数（Saint-Venantのねじり理論）
            double a = Math.Max(B, H);  // 長辺
            double b = Math.Min(B, H);  // 短辺
            
            // 矩形断面のねじり定数の近似式
            this.J = (a * Math.Pow(b, 3)) * (1.0 / 3.0 - 0.21 * (b / a) * (1.0 - Math.Pow(b, 4) / (12.0 * Math.Pow(a, 4))));
        }

        /// <summary>
        /// 中空角形鋼管の計算
        /// </summary>
        private void CalculateHollowBoxSection()
        {
            // 内側の寸法
            double b_in = B - 2 * t;  // 内側幅
            double h_in = H - 2 * t;  // 内側高さ

            // 断面積 [mm²]
            // 外側面積 - 内側面積
            this.A = B * H - b_in * h_in;

            // 断面二次モーメント [mm⁴]
            // Iyy: Y軸周りの慣性モーメント（曲げ軸がZ軸）
            this.Iyy = (B * Math.Pow(H, 3)) / 12.0 - (b_in * Math.Pow(h_in, 3)) / 12.0;

            // Izz: Z軸周りの慣性モーメント（曲げ軸がY軸）
            this.Izz = (H * Math.Pow(B, 3)) / 12.0 - (h_in * Math.Pow(b_in, 3)) / 12.0;

            // 断面二次半径 [mm]
            this.iy = Math.Sqrt(Iyy / A);
            this.iz = Math.Sqrt(Izz / A);

            // ねじり定数 [mm⁴]
            // 閉断面の場合のねじり定数（薄肉近似）
            // J = (4 * A₀²) / (Σ(s/t))
            // A₀: 中心線で囲まれた面積
            // s: 周長、t: 板厚
            double A0 = (B - t) * (H - t);  // 中心線の面積
            double perimeter = 2 * (B + H);  // 外周長
            
            // 薄肉閉断面のねじり定数
            this.J = (4.0 * A0 * A0 * t) / perimeter;
        }

        /// <summary>
        /// シリアライゼーション用データ取得
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("B", B);
            info.AddValue("H", H);
            info.AddValue("t", t);
            info.AddValue("r", r);
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            if (t <= 0)
            {
                return $"CrossSection_Box(Name={Name}(Solid))";
            }
            else
            {
                return $"CrossSection_Box(Name={Name}(Hollow))";
            }
        }

        /// <summary>
        /// クローン作成
        /// </summary>
        public override object Clone()
        {
            return new CrossSection_Box(Name, B, H, t, r);
        }
    }
}
