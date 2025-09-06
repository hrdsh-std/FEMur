using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Elements
{
    public class BeamElement : LineElement, ISerializable
    {
        public BeamElement() { }
        public BeamElement(int id, int node1Id, int node2Id, int materialId, int crossSectionId)
    : base(id, node1Id, node2Id, materialId, crossSectionId)
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
