using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Geometry
{
    public struct Face3 : IEquatable<Face3>, ISerializable
    {
        #region properties
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public bool IsQuad => this.C != this.D;
        public bool IsTriangle => this.C == this.D;
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return A;
                    case 1: return B;
                    case 2: return C;
                    case 3: return D;
                    default: throw new IndexOutOfRangeException("Face Index must be between 0 and 3.");
                }
            }

        }
        #endregion

        #region Constructors
        public Face3(int v1, int v2, int v3)
        {
            A = v1;
            B = v2;
            C = v3;
            D = v3;
        }
        public Face3(int v1, int v2, int v3, int v4)
        {
            A = v1;
            B = v2;
            C = v3;
            D = v4;
        }
        public Face3(Face3 other)
        {
            A = other.A;
            B = other.B;
            C = other.C;
            D = other.D;
        }
        public Face3(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException("Face3 Serialization not implemented yet.");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("A", A);
            info.AddValue("B", B);
            info.AddValue("C", C);
            info.AddValue("D", D);
            throw new NotImplementedException("Face3 Serialization not implemented yet.");
        }
        public override string ToString()
        {
            return $"Face3: A={A}, B={B}, C={C}, D={D}";
        }

        public bool Equals(Face3 obj)
        {
            return obj is Face3 face && this.A == A && this.B == face.B && this.C == face.C && this.D == face.D;
        }
        #endregion

    }
}
