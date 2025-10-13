using FEMur.CrossSections;
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
        public BeamElement(int id, int node1Id, int node2Id, Material material, CrossSection_Beam crossSection)
            : base(id, node1Id, node2Id, material, crossSection)
        {
        }
        public BeamElement(int id, Node node1, Node node2, Material material, CrossSection_Beam crossSection)
            : base(id, node1.Id, node2.Id, material, crossSection)
        {
        }
        public BeamElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal override Matrix<double> CalcLocalStiffness(List<Node> nodes)
        {
            int node1Id = NodeIds[0];
            int node2Id = NodeIds[1];
            var node1 = nodes.First(n => n.Id == node1Id);
            var node2 = nodes.First(n => n.Id == node2Id);

            var cs = (CrossSection_Beam)this.CrossSection;

            double E = this.Material.E;
            double G = this.Material.G;
            double A = cs.A;
            double Iyy = cs.Iyy;
            double Izz = cs.Izz;
            double J = cs.J;

            double dx = node2.Position.X - node1.Position.X;
            double dy = node2.Position.Y - node1.Position.Y;
            double dz = node2.Position.Z - node1.Position.Z;
            double L = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (L <= 1e-12) throw new ArgumentException($"Element(Id={Id}) length is approximately zero.");

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

        // 座標変換行列: v_g = T v_l（T は diag(R,R,R,R)）
        internal override Matrix<double> CalcTransformationMatrix(List<Node> nodes)
        {
            int node1Id = NodeIds[0];
            int node2Id = NodeIds[1];
            var n1 = nodes.First(n => n.Id == node1Id);
            var n2 = nodes.First(n => n.Id == node2Id);

            double dx = n2.Position.X - n1.Position.X;
            double dy = n2.Position.Y - n1.Position.Y;
            double dz = n2.Position.Z - n1.Position.Z;
            double L = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (L <= 1e-12)
                throw new ArgumentException($"Element(Id={this.Id}) length is approximately zero. Check node coordinates.");

            double exx = dx / L, exy = dy / L, exz = dz / L;

            double[] vrefArr = LocalAxis ?? new double[3] { 0, 0, 1 };
            double vx = vrefArr[0], vy = vrefArr[1], vz = vrefArr[2];

            double dot = vx * exx + vy * exy + vz * exz;
            double ezx = vx - dot * exx;
            double ezy = vy - dot * exy;
            double ezz = vz - dot * exz;
            double ezn = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);

            if (ezn < 1e-12)
            {
                vx = 0; vy = 1; vz = 0;
                dot = vx * exx + vy * exy + vz * exz;
                ezx = vx - dot * exx;
                ezy = vy - dot * exy;
                ezz = vz - dot * exz;
                ezn = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);

                if (ezn < 1e-12)
                {
                    vx = 0; vy = 0; vz = 1;
                    dot = vx * exx + vy * exy + vz * exz;
                    ezx = vx - dot * exx;
                    ezy = vy - dot * exy;
                    ezz = vz - dot * exz;
                    ezn = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);

                    if (ezn < 1e-12)
                        throw new ArgumentException($"Element(Id={this.Id}) cannot build local axes (vref parallel to element).");
                }
            }

            ezx /= ezn; ezy /= ezn; ezz /= ezn;

            double eyx = ezy * exz - ezz * exy;
            double eyy = ezz * exx - ezx * exz;
            double eyz = ezx * exy - ezy * exx;
            double eyn = Math.Sqrt(eyx * eyx + eyy * eyy + eyz * eyz);
            eyx /= eyn; eyy /= eyn; eyz /= eyn;

            // 局所座標系の基底ベクトルを保存（グローバル座標系での表現）
            LocalAxisX = new double[] { exx, exy, exz };
            LocalAxisY = new double[] { eyx, eyy, eyz };
            LocalAxisZ = new double[] { ezx, ezy, ezz };

            // グローバル→ローカル変換行列 R（行に ex, ey, ez を配置）
            var R = Matrix<double>.Build.Dense(3, 3);
            R[0, 0] = exx; R[0, 1] = exy; R[0, 2] = exz;  // 行0: ex
            R[1, 0] = eyx; R[1, 1] = eyy; R[1, 2] = eyz;  // 行1: ey
            R[2, 0] = ezx; R[2, 1] = ezy; R[2, 2] = ezz;  // 行2: ez

            // 12x12 のブロック対角 T を明示ループで構成
            var T = Matrix<double>.Build.Dense(12, 12);
            int[] bases = new[] { 0, 3, 6, 9 }; // (0,0), (3,3), (6,6), (9,9)
            foreach (var b in bases)
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        T[b + i, b + j] = R[i, j];
            }

            this.TransformationMatrix = T;
            return T;
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
               return $"BeamElement(Id:{Id}, Node1:{NodeIds[0]}, Node2:{NodeIds[1]}, Material:{Material.Name}, CrossSection:{CrossSection.Name})";
        }
    }
}
