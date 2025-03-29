using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector = System.Numerics.Vector<double>;
using FEMur.Core.Common;
using FEMur.Core.Interface;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FEMur.Core.DKTplate
{
    public class FemModel:IFemModel
    {
        public List<INode> Nodes { get; set; }
        public List<IElement> Elements { get; set; }
        public List<ISupport> Supports { get; set; }
        public List<ILoad> Loads { get; set; }


        public FemModel(List<INode> nodes, List<IElement> elements, List<ISupport> supports, List<ILoad> loads)
        {
            Nodes = nodes;
            Elements = elements;
            Supports = supports;
            Loads = loads;

            foreach (IElement element in Elements)
            {
                //要素座標への変換行列を計算する
                Matrix<double> T = ComputeElementLocalCoordinateSystem(element);
                element.SetTMatrix(T);
                //要素座標系の節点座標を計算
                Matrix<double> localCoordinates = ComputeLocalCordinates(element);
                element.SetLocalCoordinates(localCoordinates);
                //要素の面積を計算
                double area = ComputeArea(element);
                element.SetArea(area);
            }
        }

        //要素の面積を計算
        public double ComputeArea(IElement element)
        {
            INode node0 = Nodes[element.NodesID[0]];
            INode node1 = Nodes[element.NodesID[1]];
            INode node2 = Nodes[element.NodesID[2]];
            double[] v1 = new double[] { node1.X - node0.X, node1.Y - node0.Y, node1.Z - node0.Z };
            double[] v2 = new double[] { node2.X - node0.X, node2.Y - node0.Y, node2.Z - node0.Z };
            double[] n = new double[] { v1[1] * v2[2] - v1[2] * v2[1], v1[2] * v2[0] - v1[0] * v2[2], v1[0] * v2[1] - v1[1] * v2[0] };
            double area = 0.5 * Math.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
            return area;
        }


        public Matrix<double> ComputeElementLocalCoordinateSystem(IElement element)
        {
            //要素座標への変換行列を計算する
            INode node0 = Nodes[element.NodesID[0]];
            INode node1 = Nodes[element.NodesID[1]];
            INode node2 = Nodes[element.NodesID[2]];

            // 要素座標系のx軸
            double[] v1 = new double[] { node1.X - node0.X, node1.Y - node0.Y, node1.Z - node0.Z };
            double v1Norm = Math.Sqrt(v1[0] * v1[0] + v1[1] * v1[1] + v1[2] * v1[2]);//要素X軸ベクトルの正規化
            double[] ex = new double[] { v1[0] / v1Norm, v1[1] / v1Norm, v1[2] / v1Norm };

            //要素座標系のz軸
            double[] v2 = new double[] { node2.X - node0.X, node2.Y - node0.Y, node2.Z - node0.Z };
            double[] n = new double[] { v1[1] * v2[2] - v1[2] * v2[1], v1[2] * v2[0] - v1[0] * v2[2], v1[0] * v2[1] - v1[1] * v2[0] };
            double nNorm = Math.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);//要素Z軸ベクトルの正規化
            double[] ez = new double[] { n[0] / nNorm, n[1] / nNorm, n[2] / nNorm };

            //要素座標系のy軸
            double[] ey = new double[] { ez[1] * ex[2] - ez[2] * ex[1], ez[2] * ex[0] - ez[0] * ex[2], ez[0] * ex[1] - ez[1] * ex[0] };

            Matrix<double> T = Matrix<double>.Build.DenseOfArray(new double[,] { { ex[0], ex[1], ex[2] }, { ey[0], ey[1], ey[2] }, { ez[0], ez[1], ez[2] } });

            return T;
        }

        public Matrix<double> ComputeLocalCordinates(IElement element)
        {
            //変換マトリクスの計算
            Matrix<double> T = element.T;

            //要素座標系の節点座標を計算
            Matrix<double> localCoordinates = Matrix<double>.Build.Dense(element.NodesID.Count, 3);
            Matrix<double> x0 = Matrix<double>.Build.DenseOfArray(new double[,] { { Nodes[element.NodesID[0]].X, Nodes[element.NodesID[0]].Y, Nodes[element.NodesID[0]].Z } });

            for (int i = 0; i < element.NodesID.Count; i++)
            {
                INode node = Nodes[element.NodesID[i]];
                Matrix<double> globalCoordinates = Matrix<double>.Build.DenseOfArray(new double[,] { { node.X, node.Y, node.Z } });
                Matrix<double> localCoordinate = T * (globalCoordinates - x0).Transpose();
                localCoordinates.SetRow(i,localCoordinate.Column(0));
            }

            return localCoordinates;

        }
        public override string ToString()
        {
            return $"Nodes: {Nodes.Count} Elements: {Elements.Count} Supports: {Supports.Count} Loads: {Loads.Count}";
        }
    }
}
