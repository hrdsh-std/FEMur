using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.CrossSections;
using FEMur.Geometry;
using FEMur.Materials;
using MathNet.Numerics.LinearAlgebra;
using FEMur.Nodes;


namespace FEMur.Elements
{
    public class ShellElement : ElementBase,ISerializable
    {
        public Mesh3 Mesh { get; set; }
        public ShellElement(int id, CrossSection crossSection, Material material)
        {
            Id = id;
            CrossSection = crossSection;
            Material = material;
        }
        internal override Matrix<double> CalcLocalStiffness(List<Node> nodes)
        {
            throw new NotImplementedException();
        }
        internal override Matrix<double> CalcTransformationMatrix(List<Node> nodes)
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            return $"ShellElement: {Id}, Nodes: {string.Join(",", NodeIds)}";
        }
    }
}
