using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Constants;
using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Display;

namespace FEMur.Core.DKTplate
{
    public class Analyze
    {
        //DKT板用素の解析を行うクラス

        public IFemModel model { get; }

        public Result result { get; set; } = new Result();

        public Analyze(IFemModel model)
        {
            this.model = model;
            this.run();
        }

        private void run()
        {
            Matrix<double> K = calc_K();
            SetBoundaryCondition(K);
            Matrix<double> f = calc_f();
            Matrix<double> d = calc_d(K, f);

            result.SetDisplacement(d);
        }

        public Matrix<double> calc_d(Matrix<double> K, Matrix<double> f)
        {
            Matrix<double> d = K.Solve(f);
            return d;
        }

        public Matrix<double> calc_f()
        {
            Matrix<double> f = Matrix<double>.Build.Dense(model.Nodes.Count * 6, 1);
            foreach (var load in model.Loads)
            {
                if (load is NodalLoad nodalLoad)
                {
                    int nodeID = nodalLoad.nodeID;
                    f[nodeID * 6, 0] += nodalLoad.Fx;
                    f[nodeID * 6 + 1, 0] += nodalLoad.Fy;
                    f[nodeID * 6 + 2, 0] += nodalLoad.Fz;
                    f[nodeID * 6 + 3, 0] += nodalLoad.Mx;
                    f[nodeID * 6 + 4, 0] += nodalLoad.My;
                    f[nodeID * 6 + 5, 0] += nodalLoad.Mz; //強制0でもいいかも
                }
                else if (load is GravityLoad gravityLoad)
                {
                    double GravityAcc = 9.80665;
                    double gx = gravityLoad.gx;
                    double gy = gravityLoad.gy;
                    double gz = gravityLoad.gz;

                    foreach (var element in model.Elements)
                    {

                        double area = element.A;
                        double massDensity = element.Material.massDensity;
                        for (int i = 0; i < 3; i++)
                        {
                            int nodeID = element.NodesID[i];
                            f[nodeID * 6 + 0, 0] += area / 3 * massDensity * gx * GravityAcc * 1.0e-6;
                            f[nodeID * 6 + 1, 0] += area / 3 * massDensity * gy * GravityAcc * 1.0e-6;
                            f[nodeID * 6 + 2, 0] += area / 3 * massDensity * gz * GravityAcc * 1.0e-6;
                        }
                    }
                }

            }
            return f;
        }

        //public Matrix<double> CalcSectionForce()
        //{
        //    Matrix<double> d = result.d;
        //    Matrix<double> sectionForce = Matrix<double>.Build.Dense(model.Elements.Count, 6);//Fxx,Fyy,Fxy,Mxx,Myy,Mxy
        //    for (int i = 0; i < model.Elements.Count; i++)
        //    {
        //        IElement element = model.Elements[i];
        //        //膜用素のBmの計算
        //        double[] dNdx = new double[3];
        //        double[] dNdy = new double[3];

        //        double h = element.Section.Thickness;
        //        double A = GetArea(element);

        //        for (int ia = 0; ia < 3; ia++)
        //        {
        //            int ib = (ia + 1) % 3;
        //            int ic = (ia + 2) % 3;
        //            double xb = element.LocalCoordinates[ib, 0];
        //            double xc = element.LocalCoordinates[ic, 0];
        //            double yb = element.LocalCoordinates[ib, 1];
        //            double yc = element.LocalCoordinates[ic, 1];

        //            dNdx[ia] = (yb - yc) / (2 * A);
        //            dNdy[ia] = (xc - xb) / (2 * A);
        //        }

        //        Matrix<double> Bm = calc_Bm(element, dNdx, dNdy);
        //        Matrix<double> Dm = calc_Dm(element);

        //        //DKT要素のBbの計算
        //        GaussPoint[] points = GaussIntegration.GetPoints(ElementType.Triangle3, IntegrationScheme.Gauss3);
                

        //    }
        //}

        public void SetBoundaryCondition(Matrix<double> K)
        {
            foreach(ISupport support in model.Supports)
            {
                int nodeID = support.nodeID;
                if (support.DX)
                {
                    for(int i = 0; i < model.Nodes.Count; i++)
                    {
                        K[nodeID * 6, i] = 0.0;
                        K[i, nodeID * 6] = 0.0;
                    }
                    K[nodeID * 6, nodeID * 6] = 1.0e10;
                }
                if (support.DY)
                {
                    for (int i = 0; i < model.Nodes.Count; i++)
                    {
                        K[nodeID * 6 + 1, i] = 0.0;
                        K[i, nodeID * 6 + 1] = 0.0;
                    }
                    K[nodeID * 6 + 1, nodeID * 6 + 1] = 1.0e10;
                }
                if (support.DZ)
                {
                    for (int i = 0; i < model.Nodes.Count; i++)
                    {
                        K[nodeID * 6 + 2, i] = 0.0;
                        K[i, nodeID * 6 + 2] = 0.0;
                    }
                    K[nodeID * 6 + 2, nodeID * 6 + 2] = 1.0e10;
                }
                if (support.RX)
                {
                    for (int i = 0; i < model.Nodes.Count; i++)
                    {
                        K[nodeID * 6 + 3, i] = 0.0;
                        K[i, nodeID * 6 + 3] = 0.0;
                    }
                    K[nodeID * 6 + 3, nodeID * 6 + 3] = 1.0e10;
                }
                if (support.RY)
                {
                    for (int i = 0; i < model.Nodes.Count; i++)
                    {
                        K[nodeID * 6 + 4, i] = 0.0;
                        K[i, nodeID * 6 + 4] = 0.0;
                    }
                    K[nodeID * 6 + 4, nodeID * 6 + 4] = 1.0e10;
                }
                if (support.RZ)
                {
                    for (int i = 0; i < model.Nodes.Count; i++)
                    {
                        K[nodeID * 6 + 5, i] = 0.0;
                        K[i, nodeID * 6 + 5] = 0.0;
                    }
                    K[nodeID * 6 + 5, nodeID * 6 + 5] = 1.0e10;
                }
            }
        }

        //全体剛性マトリクスの計算
        public Matrix<double> calc_K()
        {
            Matrix<double>K = Matrix<double>.Build.Dense(model.Nodes.Count * 6, model.Nodes.Count * 6);
            foreach (var element in model.Elements)
            {
                Matrix<double> Ke = Calc_Ke(element); // 18x18
                
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int nodei = element.NodesID[i];
                        int nodej = element.NodesID[j];
                        for (int k = 0; k < 6; k++)
                        {
                            for (int l = 0; l < 6; l++)
                            {
                                K[nodei * 6 + k, nodej * 6 + l] += Ke[i * 6 + k, j * 6 + l];
                            }
                        }
                    }
                }
            }
            return K;
        }
        public Matrix<double> Calc_Ke(IElement element)
        {
            //CSTとDKTの重ね合わせ
            Matrix<double> Ke = Matrix<double>.Build.Dense(18, 18);
            Matrix<double> Kbe = calc_Kbe(element);
            Matrix<double> Kme = calc_Kme(element);

            for( int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    Ke[6 * i, 6 * j] = Kme[2 * i,2 * j];
                    Ke[6 * i, 6 * j + 1] = Kme[2 * i, 2 * j + 1];
                    Ke[6 * i + 1 , 6 * j] = Kme[2 * i + 1, 2 * j];
                    Ke[6 * i + 1, 6 * j + 1] = Kme[2 * i + 1, 2 * j + 1];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Ke[6 * i + 2, 6 * j + 2] = Kbe[3 * i, 3 * j];
                    Ke[6 * i + 2, 6 * j + 3] = Kbe[3 * i, 3 * j + 1];
                    Ke[6 * i + 2, 6 * j + 4] = Kbe[3 * i, 3 * j + 2];
                    Ke[6 * i + 3, 6 * j + 2] = Kbe[3 * i + 1, 3 * j];
                    Ke[6 * i + 3, 6 * j + 3] = Kbe[3 * i + 1, 3 * j + 1];
                    Ke[6 * i + 3, 6 * j + 4] = Kbe[3 * i + 1, 3 * j + 2];
                    Ke[6 * i + 4, 6 * j + 2] = Kbe[3 * i + 2, 3 * j];
                    Ke[6 * i + 4, 6 * j + 3] = Kbe[3 * i + 2, 3 * j + 1];
                    Ke[6 * i + 4, 6 * j + 4] = Kbe[3 * i + 2, 3 * j + 2];
                }
            }
            //ドリリング剛性の追加
            for (int i = 0; i < 3; i++)
            {
                double f = 0.0;
                for(int j =0; j <6; j++)
                {
                    double val = Ke[i * 6 + j, i * 6 + j];
                    if (val > f)
                    {
                        f = val;
                    }
                }
                f *= 1.0e-3;
                Ke[6 * i + 5, 6 * i + 5] = f;
            }

            //全体座標系に変換
            Matrix<double> Te = ComputeTransformationMatrix(element);
            Matrix<double> Kl = Te.Transpose() * Ke * Te;

            return Kl;
        }
        //CSTの要素剛性マトリクスの計算
        //CSTについては、定ひずみとして計算する
        public Matrix<double> calc_Kme(IElement element)
        {
            Matrix<double> Kme = Matrix<double>.Build.Dense(6, 6);
            double[] N = new double[3] { 1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0 };
            double[] dNdx = new double[3];
            double[] dNdy = new double[3];

            double h = element.Section.Thickness;
            double A = GetArea(element);

            for (int ia = 0; ia < 3; ia++)
            {
                int ib = (ia + 1) % 3;
                int ic = (ia + 2) % 3;
                double xb = element.LocalCoordinates[ib, 0];
                double xc = element.LocalCoordinates[ic, 0];
                double yb = element.LocalCoordinates[ib, 1];
                double yc = element.LocalCoordinates[ic, 1];

                dNdx[ia] = (yb - yc) / (2 * A);
                dNdy[ia] = (xc - xb) / (2 * A);
            }

            Matrix<double> Bm = calc_Bm(element, dNdx, dNdy);
            Matrix<double> Dm = calc_Dm(element);

            Kme = Bm.Transpose() * Dm * Bm * h * A;
            return Kme;
        }

        public Matrix<double> calc_Bm(IElement element, double[] dNdx, double[] dNdy)
        {
            Matrix<double> Bm = Matrix<double>.Build.Dense(3, 6);
            for (int ia = 0; ia < 3; ia++)
            {
                Bm[0, ia * 2] = dNdx[ia];
                Bm[1, ia * 2 + 1] = dNdy[ia];
                Bm[2, ia * 2] = dNdy[ia];
                Bm[2, ia * 2 + 1] = dNdx[ia];
            }
            return Bm;
        }

        public Matrix<double> calc_Dm(IElement element)
        {
            double E = element.Material.YoungModulus;
            double nu = element.Material.PoissonRatio;
            double C = E / (1 - nu * nu);
            Matrix<double> Dm = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            {
                {1.0 , nu , 0 },
                {nu , 1.0 , 0 },
                {0 , 0 , (1.0 - nu) / 2.0 }
            });

            Dm = C * Dm;
            return Dm;
        }

        //DKTの要素剛性マトリクスの計算
        private Matrix<double> calc_Kbe(IElement element)
        {
            //Kbの計算
            Matrix<double> Kb = Matrix<double>.Build.Dense(9, 9);
            Matrix<double> Db = calc_Db(element);
            double detJ = GetDetJ(element);

            //積分点を取得して、処理を行う
            GaussPoint[] points = GaussIntegration.GetPoints(ElementType.Triangle3, IntegrationScheme.Gauss3);
            foreach (var point in points)
            {
                Matrix<double> Bb = calc_Bb(element, point);
                Kb += Bb.Transpose() * Db * Bb * detJ * point.Weight * (1.0 / 2.0);
            }
            return Kb;
        }
        private Matrix<double> calc_Db(IElement element)
        {
            //Dbの計算
            Matrix<double> Db = Matrix<double>.Build.Dense(9, 9);
            double E = element.Material.YoungModulus;
            double nu = element.Material.PoissonRatio;
            double h = element.Section.Thickness;

            Matrix<double> C = Matrix<double>.Build.DenseOfArray(new double[3, 3] 
            {
                { 1.0, nu, 0.0},
                { nu, 1.0, 0.0},
                { 0.0, 0.0, (1.0 - nu) / 2.0}
            });

            Db = (E * Math.Pow(h, 3) / (12 * (1 - nu * nu))) * C;


            return Db;
        }
        private Matrix<double> calc_Bb(IElement element,GaussPoint point)
        {
            //Bbの計算
            Matrix<double> Bb = Matrix<double>.Build.Dense(3, 9);
            double[] ha;
            double[] hb;
            double[] hc;
            double[] hd;
            double[] he;
            calcHfunctionCoeffs(element,out ha,out hb,out hc,out hd,out he);

            double[] L = point.Ls;
            double weight = point.Weight;
            double[] N = calc_N(L);
            double[][] dNdu = calc_dNdu(element,L);
            double[] dNdx = dNdu[0];
            double[] dNdy = dNdu[1];
            double detJ = GetDetJ(element);

            double[] Hx;
            double[] Hy;
            double[] dHxdx;
            double[] dHxdy;
            double[] dHydx;
            double[] dHydy;
            calcHFunctions(N, dNdx, dNdy, ha, hb, hc, hd, he, out Hx, out Hy, out dHxdx, out dHxdy, out dHydx, out dHydy);

            for (int i = 0; i < 9; i++)
            {
                Bb[0, i] = dHxdx[i];
                Bb[1, i] = dHydy[i];
                Bb[2, i] = dHxdy[i] + dHydx[i];
            }
            return Bb;
        }

        public double GetDetJ(IElement element)
        {
            double x1 = element.LocalCoordinates[0, 0];
            double x2 = element.LocalCoordinates[1, 0];
            double x3 = element.LocalCoordinates[2, 0];
            double y1 = element.LocalCoordinates[0, 1];
            double y2 = element.LocalCoordinates[1, 1];
            double y3 = element.LocalCoordinates[2, 1];

            double A = GetArea(element);
            double detJ = 2.0 * A;

            return detJ;
        }
        public double GetArea(IElement element)
        {
            double x1 = element.LocalCoordinates[0, 0];
            double x2 = element.LocalCoordinates[1, 0];
            double x3 = element.LocalCoordinates[2, 0];
            double y1 = element.LocalCoordinates[0, 1];
            double y2 = element.LocalCoordinates[1, 1];
            double y3 = element.LocalCoordinates[2, 1];

            //三角形の面積を求める
            return Math.Abs((x1 - x3) * (y2 - y1) - (x1 - x2) * (y3 - y1)) / 2.0;
        }
        public void calcTransMatix(IElement element ,out double[] a,out double[] b ,out double[] c)
        {
            //XY→重心座標への変換マトリクスの計算
            //Li = ai + bi*x + ci*y
            a = new double[3];
            b = new double[3];
            c = new double[3];

            double x1 = element.LocalCoordinates[0, 0];
            double x2 = element.LocalCoordinates[1, 0];
            double x3 = element.LocalCoordinates[2, 0];
            double y1 = element.LocalCoordinates[0, 1];
            double y2 = element.LocalCoordinates[1, 1];
            double y3 = element.LocalCoordinates[2, 1];

            double A = GetArea(element);

            for (int ia = 0; ia < 3; ia++)
            {
                int ib = (ia + 1) % 3;
                int ic = (ia + 2) % 3;
                double xa = element.LocalCoordinates[ia, 0];
                double xb = element.LocalCoordinates[ib, 0];
                double xc = element.LocalCoordinates[ic, 0];
                double ya = element.LocalCoordinates[ia, 1];
                double yb = element.LocalCoordinates[ib, 1];
                double yc = element.LocalCoordinates[ic, 1];

                a[ia] = (xb * yc - xc * yb) / (2 * A);
                b[ia] = (yb - yc) / (2 * A);
                c[ia] = (xc - xb) / (2 * A);
            }
        }

        public double[] calc_N(double[] L)
        {
            double[] N = new double[6];
            N[0] = L[0] * (2 * L[0] - 1);
            N[1] = L[1] * (2 * L[1] - 1);
            N[2] = L[2] * (2 * L[2] - 1);
            N[3] = 4 * L[0] * L[1];
            N[4] = 4 * L[1] * L[2];
            N[5] = 4 * L[2] * L[0];
            return N;
        }
            
        public double[][] calc_dNdu(IElement element ,double[] L)
        {

            double[] a = new double[3];
            double[] b = new double[3];
            double[] c = new double[3];

            calcTransMatix(element, out a, out b, out c);

            double[][] dNdu = new double[2][];
            dNdu[0] = new double[6];
            dNdu[1] = new double[6];

            for (int i = 0; i < 3; i++)
            {
                dNdu[0][i] = b[i] * (4 * L[i] - 1);
                dNdu[1][i] = c[i] * (4 * L[i] - 1);
                dNdu[0][i + 3] = 4 * (b[i] * L[(i + 1) % 3] + b[(i + 1) % 3] * L[i]);
                dNdu[1][i + 3] = 4 * (c[i] * L[(i + 1) % 3] + c[(i + 1) % 3] * L[i]);
            }
            return dNdu;
        }

        private void calcHfunctionCoeffs (
            IElement element,
            out double[] ha,out double[] hb, out double[] hc, out double[] hd, out double[] he)
        {
            double[] xab = new double[3];
            double[] yab = new double[3];
            double[] lc = new double[3];
            for (int ic = 0; ic < 3; ic++)
            {
                int ia = (ic + 1) % 3;
                int ib = (ic + 2) % 3;
                double _xab = element.LocalCoordinates[ia, 0] - element.LocalCoordinates[ib, 0];
                double _yab = element.LocalCoordinates[ia, 1] - element.LocalCoordinates[ib, 1];

                double _lc = Math.Sqrt(Math.Pow(_xab, 2) + Math.Pow(_yab, 2));
                xab[ic] = _xab;
                yab[ic] = _yab;
                lc[ic] = _lc;
            }
            ha = new double[3];
            hb = new double[3];
            hc = new double[3];
            hd = new double[3];
            he = new double[3];

            for (int i = 0; i < 3; i++){
                int ic = (i + 2) % 3;
                double _xab = xab[ic];
                double _yab = yab[ic];
                double _lc = lc[ic];
                ha[i] = -(1.0 / (_lc * _lc)) * _xab;
                hb[i] = (3.0 / 4.0) * (1.0 / (_lc * _lc)) * _xab * _yab;
                hc[i] = (1.0 / (_lc * _lc)) * ((1.0 / 4.0) * _xab * _xab - (1.0 / 2.0) * _yab * _yab);
                hd[i] = -(1.0 / (_lc * _lc)) * _yab;
                he[i] = (1.0 / (_lc * _lc)) * (-(1.0 / 2.0) * _xab * _xab + (1.0 / 4.0) * _yab * _yab);
            }
        }

        private void calcHFunctions(
            double[] N, double[] dNdx, double[] dNdy,
            double[] ha, double[] hb, double[] hc, double[] hd, double[] he,
            out double[] Hx, out double[] Hy,
            out double[] dHxdx, out double[] dHxdy, out double[] dHydx, out double[] dHydy)
        {
            Hx = new double[9];
            Hy = new double[9];
            dHxdx = new double[9];
            dHxdy = new double[9];
            dHydx = new double[9];
            dHydy = new double[9];

            for (int i = 0; i < 3; i++)
            {
                int i4 = i + 3;
                int i6 = (i + 2) % 3 + 3;

                //形状関数Hx,Hyの計算
                Hx[i*3+0] = 3.0 / 2.0 * (ha[i4-3] * N[i4] - ha[i6-3] * N[i6]);
                Hx[i*3+1] = hb[i4 - 3] * N[i4] + hb[i6 - 3] * N[i6];
                Hx[i*3+2] = N[i] - hc[i4 - 3] * N[i4] - hc[i6 - 3] * N[i6];

                Hy[i*3+0] = 3.0 / 2.0 * (hd[i4 - 3] * N[i4] - hd[i6 - 3] * N[i6]);
                Hy[i*3+1] = -N[i] + he[i4 - 3] * N[i4] + he[i6 - 3] * N[i6];
                Hy[i*3+2] = -hb[i4 - 3] * N[i4] - hb[i6 - 3] * N[i6];

                //形状関数の微分dHx/dx,dHx/dy,dHy/dx,dHy/dyの計算
                dHxdx[i*3+0] = 3.0 / 2.0 * (ha[i4 - 3] * dNdx[i4] - ha[i6 - 3] * dNdx[i6]);
                dHxdx[i*3+1] = hb[i4 - 3] * dNdx[i4] + hb[i6 - 3] * dNdx[i6];
                dHxdx[i*3+2] = dNdx[i] - hc[i4 - 3] * dNdx[i4] - hc[i6 - 3] * dNdx[i6];

                dHxdy[i*3+0] = 3.0 / 2.0 * (ha[i4 - 3] * dNdy[i4] - ha[i6 - 3] * dNdy[i6]);
                dHxdy[i*3+1] = hb[i4 - 3] * dNdy[i4] + hb[i6 - 3] * dNdy[i6];
                dHxdy[i*3+2] = dNdy[i] - hc[i4 - 3] * dNdy[i4] - hc[i6 - 3] * dNdy[i6];

                dHydx[i*3+0] = 3.0 / 2.0 * (hd[i4 - 3] * dNdx[i4] - hd[i6 - 3] * dNdx[i6]);
                dHydx[i*3+1] = -dNdx[i] + he[i4 - 3] * dNdx[i4] + he[i6 - 3] * dNdx[i6];
                dHydx[i*3+2] = -hb[i4 - 3] * dNdx[i4] - hb[i6 - 3] * dNdx[i6];

                dHydy[i*3+0] = 3.0 / 2.0 * (hd[i4 - 3] * dNdy[i4] - hd[i6 - 3] * dNdy[i6]);
                dHydy[i*3+1] = -dNdy[i] + he[i4 - 3] * dNdy[i4] + he[i6 - 3] * dNdy[i6];
                dHydy[i*3+2] = -hb[i4 - 3] * dNdy[i4] - hb[i6 - 3] * dNdy[i6];

            }

        }
        //座標変換行列のメソッド
        private Matrix<double> ComputeTransformationMatrix(IElement element)
        {
            Matrix<double> Te = Matrix<double>.Build.DenseIdentity(18);
            Matrix<double> T = element.T;

            for (int i = 0; i<6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Te[i * 3 + k, i * 3 + j] = T[k, j];
                    }
                }
            }

            return Te;

        }
    }
}
