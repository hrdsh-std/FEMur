using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using FEMur.Core.Common;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Core.Interface
{
    public interface IElement
    {
        int ID { get; }
        List<int> NodesID { get; }
        ISection Section { get; }//3D要素の場合はnull
        IMaterial Material { get; }
        Matrix<double> T { get; }
        double A { get; }
        Matrix<double> LocalCoordinates { get; }
        void SetLocalCoordinates(Matrix<double> localCoordinates);
        void SetTMatrix(Matrix<double> T);
        void SetArea(double a);
        string ToString();
    }
}
