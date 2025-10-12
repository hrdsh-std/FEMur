using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;
using MathNet.Numerics.LinearAlgebra;
using FEMur.Nodes;
using FEMur.Materials;
using FEMur.CrossSections;


namespace FEMur.Elements
{
    public abstract class ElementBase : CommonObject, ICloneable, ISerializable
    {
        public int Id { get; set; }
        public List<int> NodeIds { get; set; }
        public Material Material { get; set; }
        public CrossSection CrossSection { get; set; }
        internal double Length { get; set; }
        internal double[] LocalAxis { get; set; } = new double[3] { 0, 0, 1 };
        internal Matrix<double> TransformationMatrix { get; set; }
        internal Matrix<double> LocalStiffness { get; set; }
        internal Matrix<double> GlobalStiffness { get; set; }
        internal Matrix<double> LocalMass { get; set; }
        internal Matrix<double> GlobalMass { get; set; }
        internal Matrix<double> LocalDamping { get; set; }
        internal Matrix<double> GlobalDamping { get; set; }

        protected ElementBase()
        {
        }
        protected ElementBase(int id, List<int> nodeIds, Material material, CrossSection_Beam crossSection)
        {
            Id = id;
            NodeIds = nodeIds;
            Material = material;
            CrossSection = crossSection;
        }

        protected ElementBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal abstract Matrix<double> CalcLocalStiffness(List<Node> nodes);

        internal abstract Matrix<double> CalcTransformationMatrix(List<Node> nodes);

        //Tostringの実装を強制
        public abstract override string ToString();


    }
}
