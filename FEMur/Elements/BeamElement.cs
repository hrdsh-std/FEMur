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

namespace FEMur.Elements
{
    public class BeamElement : LineElement, ISerializable
    {
        public BeamElement() { }
        public BeamElement(int id, int node1Id, int node2Id, int materialId, int crossSectionId)
    : base(id, node1Id, node2Id, materialId, crossSectionId)
        {
        }
        public BeamElement(int id, Node node1,Node node2, Material material, CrossSection_Beam crossSection)
            : base(id, new List<int> { node1.Id, node2.Id }, material.Id, crossSection.Id)
        {
        }
        public BeamElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal override Matrix<double> CalcLocalStiffness()
        {
            throw new NotImplementedException();
        }


    }
}
