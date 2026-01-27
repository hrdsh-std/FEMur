using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Results
{
    /// <summary>
    /// 節点反力クラス
    /// 拘束された節点で発生する反力とモーメントを格納
    /// </summary>
    public class ReactionForce
    {
        /// <summary>
        /// 節点ID
        /// </summary>
        public int NodeId { get; set; }

        /// <summary>
        /// X方向反力 [N]
        /// </summary>
        public double Fx { get; set; }

        /// <summary>
        /// Y方向反力 [N]
        /// </summary>
        public double Fy { get; set; }

        /// <summary>
        /// Z方向反力 [N]
        /// </summary>
        public double Fz { get; set; }

        /// <summary>
        /// X軸周りの反力モーメント [N・mm]
        /// </summary>
        public double Mx { get; set; }

        /// <summary>
        /// Y軸周りの反力モーメント [N・mm]
        /// </summary>
        public double My { get; set; }

        /// <summary>
        /// Z軸周りの反力モーメント [N・mm]
        /// </summary>
        public double Mz { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ReactionForce()
        {
        }

        /// <summary>
        /// コンストラクタ（全パラメータ指定）
        /// </summary>
        public ReactionForce(int nodeId, double fx, double fy, double fz, double mx, double my, double mz)
        {
            NodeId = nodeId;
            Fx = fx;
            Fy = fy;
            Fz = fz;
            Mx = mx;
            My = my;
            Mz = mz;
        }
        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"Node {NodeId}: F=({Fx:F3}, {Fy:F3}, {Fz:F3}) M=({Mx:F3}, {My:F3}, {Mz:F3})";
        }

        /// <summary>
        /// 指定された自由度に反力があるかどうかを判定
        /// </summary>
        /// <param name="dof">自由度インデックス (0:Fx, 1:Fy, 2:Fz, 3:Mx, 4:My, 5:Mz)</param>
        /// <param name="tolerance">判定用の許容誤差</param>
        /// <returns>反力が存在する場合true</returns>
        public bool HasReactionAt(int dof, double tolerance = 1e-10)
        {
            switch (dof)
            {
                case 0: return Math.Abs(Fx) > tolerance;
                case 1: return Math.Abs(Fy) > tolerance;
                case 2: return Math.Abs(Fz) > tolerance;
                case 3: return Math.Abs(Mx) > tolerance;
                case 4: return Math.Abs(My) > tolerance;
                case 5: return Math.Abs(Mz) > tolerance;
                default: return false;
            }
        }

        /// <summary>
        /// 指定された自由度の反力値を取得
        /// </summary>
        /// <param name="dof">自由度インデックス (0:Fx, 1:Fy, 2:Fz, 3:Mx, 4:My, 5:Mz)</param>
        /// <returns>反力値</returns>
        public double GetReactionAt(int dof)
        {
            switch (dof)
            {
                case 0: return Fx;
                case 1: return Fy;
                case 2: return Fz;
                case 3: return Mx;
                case 4: return My;
                case 5: return Mz;
                default: return 0.0;
            }
        }
    }
}