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
        /// <param name="id">断面ID</param>
        /// <param name="name">断面名</param>
        /// <param name="d">外径 [mm]</param>
        /// <param name="t">板厚 [mm]</param>
        public CrossSection_Circle(int id, string name, double d, double t)
            : base(id, name)
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
            double r_out = D / 2.0;           // 外側半径
            double d_in = D - 2 * t;          // 内径
            double r_in = d_in / 2.0;         // 内側半径

            // 断面積 [mm²]
            // A = π * (r_out² - r_in²)
            this.A = Math.PI * (r_out * r_out - r_in * r_in);

            // 断面二次モーメント [mm⁴]
            // I = (π/64) * (D⁴ - d_in⁴) = (π/4) * (r_out⁴ - r_in⁴)
            double I = Math.PI * (Math.Pow(r_out, 4) - Math.Pow(r_in, 4)) / 4.0;
            this.Iyy = I;
            this.Izz = I;  // 円形断面はY軸、Z軸で対称

            // 断面二次半径 [mm]
            // i = √(I/A)
            this.iy = Math.Sqrt(I / A);
            this.iz = Math.Sqrt(I / A);

            // ねじり定数（極断面二次モーメント）[mm⁴]
            // J = Ip = (π/32) * (D⁴ - d_in⁴) = (π/2) * (r_out⁴ - r_in⁴)
            this.J = Math.PI * (Math.Pow(r_out, 4) - Math.Pow(r_in, 4)) / 2.0;
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
            return $"CrossSection_Circle(Id={Id}, Name={Name}, D={D:F1}×t={t:F1})";
        }

        /// <summary>
        /// クローン作成
        /// </summary>
        public override object Clone()
        {
            return new CrossSection_Circle(Id, Name, D, t);
        }
    }
}
