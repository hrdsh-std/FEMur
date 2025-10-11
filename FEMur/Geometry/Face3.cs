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
            A = info.GetInt32("A");
            B = info.GetInt32("B");
            C = info.GetInt32("C");
            D = info.GetInt32("D");
        }

        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("A", A);
            info.AddValue("B", B);
            info.AddValue("C", C);
            info.AddValue("D", D);
        }

        public override string ToString()
        {
            return $"Face3: A={A}, B={B}, C={C}, D={D}";
        }

        // IEquatable<Face3>
        public bool Equals(Face3 other)
        {
            return this.A == other.A && this.B == other.B && this.C == other.C && this.D == other.D;
        }

        // object.Equals のオーバーライド
        public override bool Equals(object obj)
        {
            if (obj is Face3 other)
                return Equals(other);
            return false;
        }

        // object.GetHashCode のオーバーライド（.NET 4.8 でも動作する手動合成）
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + A.GetHashCode();
                hash = hash * 31 + B.GetHashCode();
                hash = hash * 31 + C.GetHashCode();
                hash = hash * 31 + D.GetHashCode();
                return hash;
            }
        }

        //Edge3を列挙型で返すメソッド
        public IEnumerable<Edge3> Edges()
        {
            if (IsTriangle)
            {
                yield return new Edge3(A, B);
                yield return new Edge3(B, C);
                yield return new Edge3(C, A);
                yield break;
            }
            if (IsQuad)
            {
                yield return new Edge3(A, B);
                yield return new Edge3(B, C);
                yield return new Edge3(C, D);
                yield return new Edge3(D, A);
                yield break;
            }
        }

        #region operators
        public static bool operator ==(Face3 a, Face3 b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Face3 a, Face3 b)
        {
            return !a.Equals(b);
        }
        #endregion
    }
}
