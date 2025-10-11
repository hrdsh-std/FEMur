using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using FEMur.Nodes;
using FEMur.Materials;
using FEMur.CrossSections;
using Grasshopper.Kernel.Geometry.SpatialTrees;
using FEMur.Geometry;
using Grasshopper.Kernel.Special;

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
            k11[0, 0] = ka;
            k11[1, 1] = 12 * kbz;
            k11[1, 5] = 6 * kbz * L;
            k11[2, 2] = 12 * kby;
            k11[2, 4] = -6 * kby * L;
            k11[3, 3] = kt;
            k11[4, 2] = -6 * kby * L;
            k11[4, 4] = 4 * kby * L * L;
            k11[5, 1] = 6 * kbz * L;
            k11[5, 5] = 4 * kbz * L * L;

            var k12 = Matrix<double>.Build.Dense(6, 6);
            k12[0, 0] = -ka;
            k12[1, 1] = -12 * kbz;
            k12[1, 5] = 6 * kbz * L;
            k12[2, 2] = -12 * kby;
            k12[2, 4] = -6 * kby * L;
            k12[3, 3] = -kt;
            k12[4, 2] = -6 * kby * L;
            k12[4, 4] = 2 * kby * L * L;
            k12[5, 1] = 6 * kbz * L;
            k12[5, 5] = 2 * kbz * L * L;

            var k21 = k12.Transpose();
            var k22 = k11;

            var ke = Matrix<double>.Build.Dense(12, 12);

            // 明示ループでブロック配置（SetSubMatrix に依存しない）
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    ke[r, c]       = k11[r, c]; // (0,0)
                    ke[r, c + 6]   = k12[r, c]; // (0,6)
                    ke[r + 6, c]   = k21[r, c]; // (6,0)
                    ke[r + 6, c+6] = k22[r, c]; // (6,6)
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

            var R = Matrix<double>.Build.Dense(3, 3);
            R[0, 0] = exx; R[1, 0] = exy; R[2, 0] = exz;
            R[0, 1] = eyx; R[1, 1] = eyy; R[2, 1] = eyz;
            R[0, 2] = ezx; R[1, 2] = ezy; R[2, 2] = ezz;

            // 12x12 のブロック対角 T を明示ループで構成（SetSubMatrix は使わない）
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
    }
}
