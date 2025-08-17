using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.IO;

namespace FEMur.Geometry
{
    public struct Edge3 : IEquatable<Edge3>
    {
        public int A { get; private set; }
        public int B { get; private set; }

        public int this[int index]
        {
            get
            {
                if(index == 0)
                    return A;
                else if (index == 1)
                    return B;
                else
                    throw new IndexOutOfRangeException("Edge Index must be 0 or 1.");
            }
        }

        public Edge3(int a, int b)
        {
            A = a;
            B = b;
        }
        public Edge3(Edge3 other)
        {
            A = other.A;
            B = other.B;
        }
        public bool Equals(Edge3 other)
        {
            return (A == other.A && B == other.B) || (A == other.B && B == other.A);
        }
        public override bool Equals(object other)
        {
            if (other is Edge3 edge)
            {
                return (A == edge.A && B == edge.B) || (A == edge.B && B == edge.A);
            }
            return false;
        }

        public override int GetHashCode() => Tuple.Create(A, B).GetHashCode();

        public static bool operator ==(Edge3 a, Edge3 b) => a.Equals(b);
        public static bool operator !=(Edge3 a, Edge3 b) => !a.Equals(b);

        public Edge3 Opposite() => new Edge3(B, A);
        public Edge3 Normalized() => A < B ? this : this.Opposite();
        public override string ToString()
        {
            string str1 = A.ToString();
            string str2 = B.ToString();
            return $"Edge from vertex {str1} to vertex {str2}";
        }
    }
}