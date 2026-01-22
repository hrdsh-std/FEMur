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

            double L = Length;

            double ka = E * A / L;
            double kt = G * J / L;
            double kby = E * Iyy / (L * L * L);
            double kbz = E * Izz / (L * L * L);

            var k_axial = Matrix<double>.Build.Dense(2, 2);
            var k_torsion = Matrix<double>.Build.Dense(2, 2);
            var k_bending_y = Matrix<double>.Build.Dense(4, 4);
            var k_bending_z = Matrix<double>.Build.Dense(4, 4);

            k_axial[0, 0] = ka;
            k_axial[0, 1] = -ka;
            k_axial[1, 0] = -ka;
            k_axial[1, 1] = ka;


            k_torsion[0, 0] = kt;
            k_torsion[0, 1] = -kt;
            k_torsion[1, 0] = -kt;
            k_torsion[1, 1] = kt;


            k_bending_y[0, 0] =  12 * kby;
            k_bending_y[0, 1] = - 6 * kby * L;
            k_bending_y[0, 2] = -12 * kby;
            k_bending_y[0, 3] = - 6 * kby * L;

            k_bending_y[1, 0] = - 6 * kby * L;
            k_bending_y[1, 1] =  4 * kby * L * L;
            k_bending_y[1, 2] =  6 * kby * L;
            k_bending_y[1, 3] =  2 * kby * L * L;

            k_bending_y[2, 0] = -12 * kby;
            k_bending_y[2, 1] =  6 * kby * L;
            k_bending_y[2, 2] =  12 * kby;
            k_bending_y[2, 3] =  6 * kby * L;

            k_bending_y[3, 0] = - 6 * kby * L;
            k_bending_y[3, 1] =  2 * kby * L * L;
            k_bending_y[3, 2] =  6 * kby * L;
            k_bending_y[3, 3] =  4 * kby * L * L;


            k_bending_z[0, 0] = 12 * kbz;
            k_bending_z[0, 1] = -6 * kbz * L;
            k_bending_z[0, 2] = -12 * kbz;
            k_bending_z[0, 3] = -6 * kbz * L;

            k_bending_z[1, 0] = -6 * kbz * L;
            k_bending_z[1, 1] = 4 * kbz * L * L;
            k_bending_z[1, 2] = 6 * kbz * L;
            k_bending_z[1, 3] = 2 * kbz * L * L;

            k_bending_z[2, 0] = -12 * kbz;
            k_bending_z[2, 1] = 6 * kbz * L;
            k_bending_z[2, 2] = 12 * kbz;
            k_bending_z[2, 3] = 6 * kbz * L;

            k_bending_z[3, 0] = -6 * kbz * L;
            k_bending_z[3, 1] = 2 * kbz * L * L;
            k_bending_z[3, 2] = 6 * kbz * L;
            k_bending_z[3, 3] = 4 * kbz * L * L;


            var ke = Matrix<double>.Build.Dense(12, 12);
            // 軸方向(dx 0,6)
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    ke[i * 6 + 0, j * 6 + 0] = k_axial[i, j];
                }
            }
            // ねじり(rx 3,9)
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    ke[i * 6 + 3, j * 6 + 3] = k_torsion[i, j];
                }
            }
            // 曲げY (dz 2,8 ,ry 4,10)
            ke[2,2]     = k_bending_y[0,0];
            ke[2,4]     = k_bending_y[0,1];
            ke[2,8]     = k_bending_y[0,2];
            ke[2,10]    = k_bending_y[0,3];
            ke[4,2]     = k_bending_y[1,0];
            ke[4,4]     = k_bending_y[1,1];
            ke[4,8]     = k_bending_y[1,2];
            ke[4,10]    = k_bending_y[1,3];
            ke[8,2]     = k_bending_y[2,0];
            ke[8,4]     = k_bending_y[2,1];
            ke[8,8]     = k_bending_y[2,2];
            ke[8,10]    = k_bending_y[2,3];
            ke[10,2]    = k_bending_y[3,0];
            ke[10,4]    = k_bending_y[3,1];
            ke[10,8]    = k_bending_y[3,2];
            ke[10,10]   = k_bending_y[3,3];

            // 曲げZ(dy 1,7, rz 5,11)
            ke[1,1]     = k_bending_z[0,0];
            ke[1,5]     = k_bending_z[0,1];
            ke[1,7]     = k_bending_z[0,2];
            ke[1,11]    = k_bending_z[0,3];
            ke[5,1]     = k_bending_z[1,0];
            ke[5,5]     = k_bending_z[1,1];
            ke[5,7]     = k_bending_z[1,2];
            ke[5,11]    = k_bending_z[1,3];
            ke[7,1]     = k_bending_z[2,0];
            ke[7,5]     = k_bending_z[2,1];
            ke[7,7]     = k_bending_z[2,2];
            ke[7,11]    = k_bending_z[2,3];
            ke[11,1]    = k_bending_z[3,0];
            ke[11,5]    = k_bending_z[3,1];
            ke[11,7]    = k_bending_z[3,2];
            ke[11,11]   = k_bending_z[3,3];

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
