using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Core.Model
{
    public class Material
    {
        double E { get; }
        double nu { get; }
        public Matrix<double> D2d { get; }//2次元要素のDマトリクス



        public Material(double E, double nu)
        {
            this.E = E;
            this.nu = nu;
            this.D2d = calc_D2d();
        }

        public override string ToString()
        {
            return $"E {E}, NU {nu}";
        }

        public Matrix<double> calc_D2d()
        {
            Matrix<double> D2d = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {1, this.nu, 0},
                {this.nu, 1, 0},
                {0, 0, (1-this.nu)/2}
            });

            D2d = D2d * this.E / (1 - Math.Pow(this.nu, 2));
            return D2d;
        }
    }
}
