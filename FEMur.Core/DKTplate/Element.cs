using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Interface;
using FEMur.Core.Common;
using Eto.Drawing;
using MathNet.Numerics.LinearAlgebra;


namespace FEMur.Core.DKTplate
{
    public class Element : IElement
    {
        public int ID { get; }
        public List<int> NodesID { get; }
        public IMaterial Material { get; }
        public ISection Section { get; }
        public Matrix<double>? LocalCoordinates { get; private set; } // 要素座標系の節点座標
        public Matrix<double>? T { get; private set; } // 要素座標系への変換行列
        public double A { get; private set; }//面積

        public Element(int id, List<int> nodesID, IMaterial material, ISection section)
        {
            ID = id;
            NodesID = nodesID;
            Material = material;
            Section = section;
            T = null;
            LocalCoordinates = null;
        }

        public void SetLocalCoordinates(Matrix<double> localCoordinates)
        {
            LocalCoordinates = localCoordinates;
        }
        public void SetTMatrix(Matrix<double> TMatrix)
        {
            T = TMatrix;
        }
        public override string ToString()
        {
            return $"DKT3PlateElement {ID}: Node {NodesID} Material {Material} Section {Section}";
        }
        public void SetArea(double a)
        {
            A = a;
        }
    }
}
