using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Common;
using FEMur.Core.Interface;
using MathNet.Numerics.LinearAlgebra;


namespace FEMur.Core.DKTplate
{
    public class Result
    {
        //解析結果を格納するクラス
        public Matrix<double>? d { get; set; }
        public Matrix<double>? stress { get; set; }
        public double[]? misesStress { get; set; }
        public Matrix<double>? principalStress { get; set; }
        public double[]? avgPrincipalStress { get; set; }
        public double[]? maxShearStress { get; set; }
        public Matrix<double>? axialForce { get; set; }
        public Matrix<double>? shearForce { get; set; }
        public Matrix<double>? bendingMoment { get; set; }

        public MathNet.Numerics.LinearAlgebra.Vector<Complex> eigenvalues { get; set; }
        public MathNet.Numerics.LinearAlgebra.Matrix<double> eigenvectors { get; set; }



        public Result()
        {
        }

        public void SetDisplacement(Matrix<double> d)
        {
            this.d = d;
        }
        public void SetEigen(MathNet.Numerics.LinearAlgebra.Vector<Complex> eigenvalues, MathNet.Numerics.LinearAlgebra.Matrix<double> eigenvectors)
        {
            this.eigenvalues = eigenvalues;
            this.eigenvectors = eigenvectors;
        }

        public void AddStress(Matrix<double> stress)
        {
            this.stress = stress;
            //主応力の計算
            Matrix<double> pStress = Matrix<double>.Build.Dense(stress.RowCount, 2);
            for (int i = 0; i < stress.RowCount; i++)
            {
                double sigx = stress[i, 0];
                double sigy = stress[i, 1];
                double tauxy = stress[i, 2];

                double p1 = (sigx + sigy) / 2 + Math.Sqrt(Math.Pow((sigx - sigy) / 2, 2) + Math.Pow(tauxy, 2));
                double p2 = (sigx + sigy) / 2 - Math.Sqrt(Math.Pow((sigx - sigy) / 2, 2) + Math.Pow(tauxy, 2));

                pStress[i, 0] = p1;
                pStress[i, 1] = p2;
            }
            this.principalStress = pStress;

            //ミーゼス応力の計算
            double[] mises = new double[stress.RowCount];
            for (int i = 0; i < stress.RowCount; i++)
            {
                double p1 = pStress[i, 0];
                double p2 = pStress[i, 1];
                mises[i] = Math.Sqrt(p1 * p1 + p2 * p2 - p1 * p2);
            }
            this.misesStress = mises;

            //平均主応力の計算
            double[] avgPrincipal = new double[stress.RowCount];
            for (int i = 0; i < stress.RowCount; i++)
            {
                double p1 = pStress[i, 0];
                double p2 = pStress[i, 1];
                avgPrincipal[i] = (p1 + p2) / 2;
            }
            this.avgPrincipalStress = avgPrincipal;

            //最大せん断応力の計算
            double[] maxShear = new double[stress.RowCount];
            for (int i = 0; i < stress.RowCount; i++)
            {
                double sigx = stress[i, 0];
                double sigy = stress[i, 1];
                double tauxy = stress[i, 2];

                maxShear[i] = Math.Sqrt(Math.Pow((sigx - sigy) / 2, 2) + tauxy * tauxy);
            }
            this.maxShearStress = maxShear;

            //軸力の計算


        }
    }

}
