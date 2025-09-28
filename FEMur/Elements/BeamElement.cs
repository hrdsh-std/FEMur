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
        public BeamElement(int id, Node node1,Node node2, Material material, CrossSection_Beam crossSection)
            : base(id,node1.Id,node2.Id , material, crossSection)
        {
        }
        public BeamElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal override Matrix<double> CalcLocalStiffness(List<Node> nodes)
        {
            //要素剛性マトリクスの計算をどう実装するか。
            //入力にNodeのリスト、マテリアルのリスト、断面のリストを受け取る必要がある。
            //外部から呼び出す想定
            int node1Id = NodeIds[0];
            int node2Id = NodeIds[1];
            var node1 = nodes[node1Id];
            var node2 = nodes[node2Id];

            CrossSection_Beam crossSection_Beam = (CrossSection_Beam)this.CrossSection;

            double E = this.Material.E;
            double A = crossSection_Beam.A;
            double Iyy = crossSection_Beam.Iyy;
            double Izz = crossSection_Beam.Izz;
            double G = this.Material.G;
            double J = crossSection_Beam.J;

            double x1 = node1.Position.X;
            double y1 = node1.Position.Y;
            double z1 = node1.Position.Z;
            double x2 = node2.Position.X;
            double y2 = node2.Position.Y;
            double z2 = node2.Position.Z;
            Point3 vec = node1.Position - node2.Position;
            double L = vec.Length;

            //軸剛性
            double ka = E * A / L;
            //ねじり剛性
            double kt = G * J / L;
            //曲げ剛性
            double kby = E * Iyy / (L * L * L);
            double kbz = E * Izz / (L * L * L);

            Matrix<double> k11 = Matrix<double>.Build.Dense(6, 6);
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

            Matrix<double> k12 = Matrix<double>.Build.Dense(6, 6);
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

            Matrix<double> k21 = k12.Transpose();
            Matrix<double> k22 = k11;

            Matrix<double> ke = Matrix<double>.Build.Dense(12, 12);
            ke.SetSubMatrix(0, 0, k11);
            ke.SetSubMatrix(0, 6, k12);
            ke.SetSubMatrix(6, 0, k21);
            ke.SetSubMatrix(6, 6, k22);

            return ke;
        }
    }
}
