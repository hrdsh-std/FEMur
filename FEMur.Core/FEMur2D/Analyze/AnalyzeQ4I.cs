using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Security.Cryptography.X509Certificates;
using FEMur.Core.FEMur2D.Model;


namespace FEMur.Core.FEMur2D.Analyze
{
    public class AnalyzeQ4I
    {
        //引数のFEMmodelの解析をして、model.resultに結果を格納するクラス

        public FEMModel model { get; }
        double[,] gps { get; set; }
        public Matrix<double> K { get; set; }
        public Matrix<double> f { get; set; }
        public AnalyzeQ4I(FEMModel model)
        {
            this.model = model;
            gps = getGps();
            K = calc_K();
            f = calc_f();
            this.model.result.d = calc_d(K, f);
            this.model.result.stress = calc_stress();
        }
        //Jmatrixを計算するメソッド
        public Matrix<double> Calc_J(Element element, double xi, double eta)
        {
            Matrix<double> dndxi = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    {(-1+eta)/4,(1-eta)/4,(1+eta)/4,(-1-eta)/4 }
            });
            Matrix<double> dndeta = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    {(-1+xi)/4,(-1-xi)/4,(1+xi)/4,(1-xi)/4 }
            });
            Matrix<double> x = Matrix<double>.Build.Dense(4, 1);
            Matrix<double> y = Matrix<double>.Build.Dense(4, 1);

            foreach (var item in element.nodes_id.Select((node_id, index) => new { node_id, index }))
            {
                x[item.index, 0] = model.nodes[item.node_id].x;
                y[item.index, 0] = model.nodes[item.node_id].y;
            }

            Matrix<double> dxdxi = dndxi * x;
            Matrix<double> dydxi = dndxi * y;
            Matrix<double> dxdeta = dndeta * x;
            Matrix<double> dydeta = dndeta * y;

            Matrix<double> J = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {dxdxi[0,0],dydxi[0,0]},
                {dxdeta[0, 0],dydeta[0, 0]}
            });
            return J;
        }
        //Bcマトリクス(3x8)を計算するメソッド
        public Matrix<double> Calc_Bc(Element element, double xi, double eta)
        {
            Matrix<double> dndxi = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    {(-1+eta)/4,(1-eta)/4,(1+eta)/4,(-1-eta)/4 }
            });
            Matrix<double> dndeta = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    {(-1+xi)/4,(-1-xi)/4,(1+xi)/4,(1-xi)/4 }
            });
            Matrix<double> x = Matrix<double>.Build.Dense(4, 1);
            Matrix<double> y = Matrix<double>.Build.Dense(4, 1);

            foreach (var item in element.nodes_id.Select((node_id, index) => new { node_id, index }))
            {
                x[item.index, 0] = model.nodes[item.node_id].x;
                y[item.index, 0] = model.nodes[item.node_id].y;
            }
            Matrix<double> dxdxi = dndxi * x;
            Matrix<double> dydxi = dndxi * y;
            Matrix<double> dxdeta = dndeta * x;
            Matrix<double> dydeta = dndeta * y;
            Matrix<double> J = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {dxdxi[0,0],dydxi[0,0]},
                {dxdeta[0, 0],dydeta[0, 0]}
            });
            Matrix<double> invJ = J.Inverse();

            Matrix<double> Bc = Matrix<double>.Build.Dense(3, 8);

            for (int i = 0; i < 4; i++)
            {
                Matrix<double> Bi = invJ * Matrix<double>.Build.DenseOfArray(new double[2, 1] { { dndxi[0, i] }, { dndeta[0, i] } });
                Bc[0, 2 * i] = Bi[0, 0];
                Bc[1, 2 * i + 1] = Bi[1, 0];
                Bc[2, 2 * i] = Bi[1, 0];
                Bc[2, 2 * i + 1] = Bi[0, 0];
            }
            return Bc;
        }
        //Biマトリクス(3x4)を計算するメソッド
        public Matrix<double> calc_Bi(Element element, double xi, double eta)
        {
            Matrix<double> J = Calc_J(element, xi, eta);
            Matrix<double> invJ = J.Inverse();
            Matrix<double> Bi = Matrix<double>.Build.DenseOfArray(new double[3, 4]
            {
                {-2*xi*invJ[0,0],-2*eta*invJ[0,1],0,0},
                {0,0,-2*xi*invJ[1,0],-2*eta*invJ[1,1]},
                {-2*xi*invJ[1,0],-2*eta*invJ[1,1],-2*xi*invJ[0,0],-2*eta*invJ[0,1]}
            });
            return Bi;
        }

        //要素剛性マトリクスKe Beを計算するメソッド
        public void calc_KeBe(Element element, double[,] gps, double _xi, double _eta, out Matrix<double> Ke, out Matrix<double> Be)
        {
            Matrix<double> Kcc = Matrix<double>.Build.Dense(8, 8);
            Matrix<double> Kci = Matrix<double>.Build.Dense(8, 4);
            Matrix<double> Kic = Matrix<double>.Build.Dense(4, 8);
            Matrix<double> Kii = Matrix<double>.Build.Dense(4, 4);

            for (int i = 0; i < gps.GetLength(0); i++)
            {
                double xi = gps[i, 0];
                double eta = gps[i, 1];
                double wi = gps[i, 2];
                double wj = gps[i, 3];
                Matrix<double> J = Calc_J(element, xi, eta);
                Matrix<double> Bc = Calc_Bc(element, xi, eta);
                Matrix<double> Bi = calc_Bi(element, xi, eta);
                Matrix<double> BcT = Bc.Transpose();
                Matrix<double> BiT = Bi.Transpose();

                Kcc += wi * wj * BcT * element.material.D2d * Bc * J.Determinant() * element.section.thickness;
                Kci += wi * wj * BcT * element.material.D2d * Bi * J.Determinant() * element.section.thickness;
                Kic += wi * wj * BiT * element.material.D2d * Bc * J.Determinant() * element.section.thickness;
                Kii += wi * wj * BiT * element.material.D2d * Bi * J.Determinant() * element.section.thickness;
            }
            Ke = Kcc - Kci * Kii.Inverse() * Kic;
            Be = Calc_Bc(element, _xi, _eta) - calc_Bi(element, _xi, _eta) * Kii.Inverse() * Kic;
        }
        //全体剛性マトリクスKを計算するメソッド
        public Matrix<double> calc_K()
        {
            Matrix<double> K = Matrix<double>.Build.Dense(model.nodes.Count * 2, model.nodes.Count * 2);
            foreach (Element element in model.elements)
            {
                Matrix<double> Ke = Matrix<double>.Build.Dense(8, 8);
                calc_KeBe(element, gps, 1, 1, out Ke, out _);

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        K[2 * element.nodes_id[i], 2 * element.nodes_id[j]] += Ke[2 * i, 2 * j];
                        K[2 * element.nodes_id[i], 2 * element.nodes_id[j] + 1] += Ke[2 * i, 2 * j + 1];
                        K[2 * element.nodes_id[i] + 1, 2 * element.nodes_id[j]] += Ke[2 * i + 1, 2 * j];
                        K[2 * element.nodes_id[i] + 1, 2 * element.nodes_id[j] + 1] += Ke[2 * i + 1, 2 * j + 1];
                    }
                }
            }

            //境界条件を考慮した全体剛性マトリクス
            foreach (Support support in model.supports)
            {
                if (support.DX)
                {
                    for (int j = 0; j < model.nodes.Count * 2; j++)
                    {
                        K[support.node.id * 2, j] = 0.0;
                        K[j, support.node.id * 2] = 0.0;
                    }
                    K[support.node.id * 2, support.node.id * 2] = 1.0;
                }
                if (support.DY)
                {
                    for (int j = 0; j < model.nodes.Count * 2; j++)
                    {
                        K[support.node.id * 2 + 1, j] = 0.0;
                        K[j, support.node.id * 2 + 1] = 0.0;
                    }
                    K[support.node.id * 2 + 1, support.node.id * 2 + 1] = 1.0;
                }
            }
            return K;
        }
        //外力ベクトルを計算するメソッド
        public Matrix<double> calc_f()
        {
            Matrix<double> f = Matrix<double>.Build.Dense(model.nodes.Count * 2, 1);
            foreach (Load load in model.loads)
            {
                f[load.node.id * 2, 0] = load.Fx;
                f[load.node.id * 2 + 1, 0] = load.Fy;
            }
            return f;
        }
        //変位を計算するメソッド
        public Matrix<double> calc_d(Matrix<double> K, Matrix<double> f)
        {
            Matrix<double> d = Matrix<double>.Build.Dense(model.nodes.Count * 2, 1);
            d = K.Inverse() * f;
            return d;
        }
        //積分点を設定するメソッド
        public double[,] getGps()
        {
            if (model.elementType == 1 || model.elementType == 2)
            {
                double[,] gps = new double[,]
                {
                    {-1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 1.0, 1.0 },
                    { 1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 1.0, 1.0},
                    { 1.0 / Math.Sqrt(3.0), 1.0 / Math.Sqrt(3.0), 1.0, 1.0},
                    { -1.0 / Math.Sqrt(3.0), 1.0 / Math.Sqrt(3.0), 1.0, 1.0}//(xi,eta,wi,wj)
                };
                return gps;
            }
            else
            {

                double[,] gps = new double[,]
                {
                    {-Math.Sqrt(3.0/5.0), -Math.Sqrt(3.0/5.0),5.0/9.0, 5.0/9.0 },
                    { 0.0, -Math.Sqrt(3.0/5.0),8.0/9.0, 5.0/9.0 },
                    { Math.Sqrt(3.0/5.0), -Math.Sqrt(3.0/5.0),5.0/9.0, 5.0/9.0 },
                    {-Math.Sqrt(3.0/5.0), 0.0,5.0/9.0, 8.0/9.0 },
                    { 0.0, 0.0,8.0/9.0, 8.0/9.0 },
                    { Math.Sqrt(3.0/5.0), 0.0,5.0/9.0, 8.0/9.0 },
                    {-Math.Sqrt(3.0/5.0), Math.Sqrt(3.0/5.0),5.0/9.0, 5.0/9.0 },
                    { 0.0, Math.Sqrt(3.0/5.0),8.0/9.0, 5.0/9.0 },
                    { Math.Sqrt(3.0/5.0), Math.Sqrt(3.0/5.0),5.0/9.0, 5.0/9.0 }
                };
                return gps;
            }

        }
        //節点変位dから節点応力を計算するメソッド(節点数x3matrix)
        public Matrix<double> calc_stress()
        {
            Matrix<double> stress = Matrix<double>.Build.Dense(model.nodes.Count, 8);
            List<int> node_count = Enumerable.Repeat(0, model.nodes.Count).ToList();

            foreach (Element element in model.elements)
            {
                Matrix<double> d_elem = Matrix<double>.Build.Dense(element.nodes_id.Count * 2, 1);
                foreach (var item in element.nodes_id.Select((node_id, index) => new { node_id, index }))
                {
                    d_elem[item.index * 2, 0] = model.result.d[item.node_id * 2, 0];
                    d_elem[item.index * 2 + 1, 0] = model.result.d[item.node_id * 2 + 1, 0];
                }
                Matrix<double> stress_elem = Matrix<double>.Build.Dense(element.nodes_id.Count, 3);//1次要素であればひとまずこれでOK、2次要素は要修正
                for (int i = 0; i < gps.GetLength(0); i++)
                {
                    double xi = gps[i, 0];
                    double eta = gps[i, 1];
                    Matrix<double> B = Matrix<double>.Build.Dense(3, 8);
                    calc_KeBe(element, gps, xi, eta, out _, out B);
                    Matrix<double> stress_gp = element.material.D2d * B * d_elem;//積分点での応力を算定3x1matrix
                    for (int j = 0; j < 3; j++)
                    {
                        stress_elem[i, j] += stress_gp[j, 0];
                    }
                }

                //積分点応力から節点応力を計算
                Matrix<double> N = Matrix<double>.Build.DenseOfArray(new double[4, 4]{
                    { 2 + Math.Sqrt(3.0),-1,2 - Math.Sqrt(3.0),-1},
                    { -1,2 + Math.Sqrt(3.0),-1,2 - Math.Sqrt(3.0)},
                    {2 - Math.Sqrt(3.0),-1,2 + Math.Sqrt(3.0),-1},
                    { -1,2 - Math.Sqrt(3.0),-1,2 + Math.Sqrt(3.0)}
                });

                Matrix<double> stress_node = N * stress_elem; //4x3matrix要素内の節点

                foreach (var item in element.nodes_id.Select((node_id, index) => new { node_id, index }))
                {
                    double sigxx = stress_node[item.index, 0];
                    double sigyy = stress_node[item.index, 1];
                    double tauxy = stress_node[item.index, 2];
                    double p1 = (sigxx + sigyy) / 2.0 + Math.Sqrt(Math.Pow(sigxx - sigyy, 2.0) / 4.0 + Math.Pow(tauxy, 2.0));
                    double p2 = (sigxx + sigyy) / 2.0 - Math.Sqrt(Math.Pow(sigxx - sigyy, 2.0) / 4.0 + Math.Pow(tauxy, 2.0));
                    double mises = Math.Sqrt(Math.Pow(p1, 2.0) + Math.Pow(p2, 2.0) - p1 * p2);
                    double avgStress = (p1 + p2) / 2.0;
                    double tauMax = Math.Abs(p1 - p2) / 2.0;

                    node_count[item.node_id] += 1;
                    stress[item.node_id, 0] += sigxx;
                    stress[item.node_id, 1] += sigyy;
                    stress[item.node_id, 2] += tauxy;
                    stress[item.node_id, 3] += p1;
                    stress[item.node_id, 4] += p2;
                    stress[item.node_id, 5] += mises;
                    stress[item.node_id, 6] += avgStress;
                    stress[item.node_id, 7] += tauMax;
                }
            }
            for (int i = 0; i < model.nodes.Count; i++)
            {
                stress[i, 0] /= node_count[i];
                stress[i, 1] /= node_count[i];
                stress[i, 2] /= node_count[i];
                stress[i, 3] /= node_count[i];
                stress[i, 4] /= node_count[i];
                stress[i, 5] /= node_count[i];
                stress[i, 6] /= node_count[i];
                stress[i, 7] /= node_count[i];
            }

            return stress;
        }
    }
}
