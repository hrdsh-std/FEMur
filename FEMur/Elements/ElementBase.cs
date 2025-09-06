using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Elements
{
    public abstract class ElementBase: CommonObject,ICloneable,ISerializable
    {
        public int Id { get; set; }
        public int[] NodeIds { get; set; }
        public int MaterialId { get; set; }
        public int CrossSectionId { get; set; }
        public double Length { get; set; }
        public double[] LocalAxis { get; set; } = new double[3] { 0, 0, 1 };
        public Matrix<double> TransformationMatrix { get; set; }
        public Matrix<double> LocalStiffness { get; set; }
        public Matrix<double> GlobalStiffness { get; set; }
        public Matrix<double> LocalMass { get; set; }
        public Matrix<double> GlobalMass { get; set; }
        public Matrix<double> LocalDamping { get; set; }
        public Matrix<double> GlobalDamping { get; set; }
        public ElementType Type { get; protected set; }
        public enum ElementType
        {
            Truss,
            Beam,
            Frame,
            Shell,
            Solid
        }

        protected ElementBase()
        {
        }

        protected ElementBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal abstract Matrix<double> CalcLocalStiffness();
    }
}
