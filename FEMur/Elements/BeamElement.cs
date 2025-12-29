using FEMur.CrossSections;
using FEMur.Geometry;
using FEMur.Materials;
using FEMur.Nodes;
using FEMur.Results;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FEMur.Elements
{
    public class BeamElement : LineElement, ISerializable
    {
        public BeamElement() { }

        // IDなしコンストラクタ（推奨）
        public BeamElement(Point3 point1, Point3 point2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(point1, point2, material, crossSection, betaAngle)
        {
        }

        public BeamElement(Node node1, Node node2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(node1, node2, material, crossSection, betaAngle)
        {
        }

        public BeamElement(List<int> nodeIds, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(nodeIds, material, crossSection, betaAngle)
        {
        }

        // ID指定コンストラクタ（既存コードとの互換性のため）
        public BeamElement(int id, int node1Id, int node2Id, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(id, node1Id, node2Id, material, crossSection, betaAngle)
        {
        }
        
        public BeamElement(int id, Node node1, Node node2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(id, node1.Id, node2.Id, material, crossSection, betaAngle)
        {
        }

        public BeamElement(int id, Point3 point1, Point3 point2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(id, point1, point2, material, crossSection, betaAngle)
        {
        }
        
        public BeamElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal override Matrix<double> CalcLocalStiffness(List<Node> nodes)
        {
            var cs = (CrossSection_Beam)this.CrossSection;

            double E = this.Material.E;
            double G = this.Material.G;
            double A = cs.A;
            double Iyy = cs.Iyy;
            double Izz = cs.Izz;
            double J = cs.J;

            // CalcLocalAxisで計算済みの部材長を使用
            double L = Length;

            double ka = E * A / L;
            double kt = G * J / L;
            double kby = E * Iyy / (L * L * L);
            double kbz = E * Izz / (L * L * L);

            var k11 = Matrix<double>.Build.Dense(6, 6);
            //i端UX
            k11[0, 0] = ka;//
            //i端UY
            k11[1, 1] = 12 * kbz;//
            k11[1, 5] = 6 * kbz * L;//
            //i端UZ
            k11[2, 2] = 12 * kby;//
            k11[2, 4] = -6 * kby * L;//
            //i端RX
            k11[3, 3] = kt;//
            //i端RY
            k11[4, 2] = -6 * kby * L;//
            k11[4, 4] = 4 * kby * L * L;//
            //i端RZ
            k11[5, 1] = 6 * kbz * L;//
            k11[5, 5] = 4 * kbz * L * L;//

            var k22 = Matrix<double>.Build.Dense(6, 6);

            //j端UX
            k22[0, 0] = ka; //
            //j端UY
            k22[1, 1] = 12 * kbz; //
            k22[1, 5] = -6 * kbz * L; //
            //j端UZ
            k22[2, 2] = 12 * kby; //
            k22[2, 4] = 6 * kby * L; //
            //j端RX
            k22[3, 3] = kt; //
            //j端RY
            k22[4, 2] = 6 * kby * L; //
            k22[4, 4] = 4 * kby * L * L; //
            //j端RZ
            k22[5, 1] = -6 * kbz * L; //
            k22[5, 5] = 4 * kbz * L * L; //

            var k12 = Matrix<double>.Build.Dense(6, 6);

            k12[0, 0] = -ka;

            k12[1, 1] = -12 * kbz;
            k12[1, 5] = 6 * kbz * L;

            k12[2, 2] = -12 * kby;
            k12[2, 4] = -6 * kby * L;

            k12[3, 3] = -kt;

            k12[4, 2] = 6 * kby * L;
            k12[4, 4] = 2 * kby * L * L;

            k12[5, 1] = -6 * kbz * L;
            k12[5, 5] = 2 * kbz * L * L;

            var k21 = k12.Transpose();

            var ke = Matrix<double>.Build.Dense(12, 12);

            // 明示ループでブロック配置（SetSubMatrix に依存しない）
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    ke[r, c] = k11[r, c]; // (0,0)
                    ke[r, c + 6] = k12[r, c]; // (0,6)
                    ke[r + 6, c] = k21[r, c]; // (6,0)
                    ke[r + 6, c + 6] = k22[r, c]; // (6,6)
                }
            }

            return ke;
        }

        /// <summary>
        /// 要素の局所変位ベクトルから断面力（応力）を計算
        /// </summary>
        /// <param name="localDisplacements">局所座標系の変位ベクトル [12x1]</param>
        /// <param name="nodes">節点リスト</param>
        /// <returns>要素の断面力</returns>
        public ElementStress CalcElementStress(Vector<double> localDisplacements, List<Node> nodes)
        {
            // 局所剛性行列を取得
            Matrix<double> keLocal = CalcLocalStiffness(nodes);

            // 局所座標系の断面力: f_local = K_local * u_local
            Vector<double> forces = keLocal * localDisplacements;

            var stress = new Results.ElementStress(this.Id);

            // i端（節点1）の断面力
            stress.Fx_i = forces[0];  // 軸力
            stress.Fy_i = forces[1];  // せん断力Y
            stress.Fz_i = forces[2];  // せん断力Z
            stress.Mx_i = forces[3];  // ねじりモーメント
            stress.My_i = forces[4];  // 曲げモーメントY
            stress.Mz_i = forces[5];  // 曲げモーメントZ

            // j端（節点2）の断面力
            stress.Fx_j = forces[6];
            stress.Fy_j = forces[7];
            stress.Fz_j = forces[8];
            stress.Mx_j = forces[9];
            stress.My_j = forces[10];
            stress.Mz_j = forces[11];

            return stress;
        }

        // Tostringのoverride
        public override string ToString()
        {
            return $"BeamElement(From:{Points[0].ToString()},To:{Points[1].ToString()}, Material:{Material.Name}, CrossSection:{CrossSection.Name}, β:{BetaAngle:F1}°)";
        }
    }
}
