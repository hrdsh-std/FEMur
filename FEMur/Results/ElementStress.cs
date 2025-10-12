using System;

namespace FEMur.Results
{
    /// <summary>
    /// 梁要素の端部応力（断面力）を格納するクラス
    /// </summary>
    public class ElementStress
    {
        /// <summary>
        /// 要素ID
        /// </summary>
        public int ElementId { get; set; }

        // i端（始点）の断面力
        /// <summary>
        /// i端 軸力 [N]
        /// </summary>
        public double Fx_i { get; set; }

        /// <summary>
        /// i端 せん断力Y [N]
        /// </summary>
        public double Fy_i { get; set; }

        /// <summary>
        /// i端 せん断力Z [N]
        /// </summary>
        public double Fz_i { get; set; }

        /// <summary>
        /// i端 ねじりモーメント [N?mm]
        /// </summary>
        public double Mx_i { get; set; }

        /// <summary>
        /// i端 曲げモーメントY [N?mm]
        /// </summary>
        public double My_i { get; set; }

        /// <summary>
        /// i端 曲げモーメントZ [N?mm]
        /// </summary>
        public double Mz_i { get; set; }

        // j端（終点）の断面力
        /// <summary>
        /// j端 軸力 [N]
        /// </summary>
        public double Fx_j { get; set; }

        /// <summary>
        /// j端 せん断力Y [N]
        /// </summary>
        public double Fy_j { get; set; }

        /// <summary>
        /// j端 せん断力Z [N]
        /// </summary>
        public double Fz_j { get; set; }

        /// <summary>
        /// j端 ねじりモーメント [N?mm]
        /// </summary>
        public double Mx_j { get; set; }

        /// <summary>
        /// j端 曲げモーメントY [N?mm]
        /// </summary>
        public double My_j { get; set; }

        /// <summary>
        /// j端 曲げモーメントZ [N?mm]
        /// </summary>
        public double Mz_j { get; set; }

        public ElementStress() { }

        public ElementStress(int elementId)
        {
            ElementId = elementId;
        }

        public override string ToString()
        {
            return $"Element {ElementId}: " +
                   $"i端[Fx={Fx_i:F2}, Fy={Fy_i:F2}, Fz={Fz_i:F2}, Mx={Mx_i:F2}, My={My_i:F2}, Mz={Mz_i:F2}], " +
                   $"j端[Fx={Fx_j:F2}, Fy={Fy_j:F2}, Fz={Fz_j:F2}, Mx={Mx_j:F2}, My={My_j:F2}, Mz={Mz_j:F2}]";
        }
    }
}