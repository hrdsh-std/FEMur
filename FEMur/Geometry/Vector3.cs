using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace FEMur.Geometry
{
    public struct Vector3 : IEquatable<Vector3>,IComparable<Vector3>, ISerializable
    {
        private const double Tolerance = 1e-10;
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double this[int index]
        {
            get
            {
                switch(index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new IndexOutOfRangeException("Index must be between 0 and 2.");
                }
            }
            set
            {
                switch(index)
                {
                    case 0: X = value;
                            break;
                    case 1: Y = value;
                            break;
                    case 2: Z = value;
                        break;
                    default: throw new IndexOutOfRangeException("Index must be between 0 and 2.");
                }
            }
        }
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3(Vector3 other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }
        public bool Equals(Vector3 other)
        {
            return Math.Abs(X - other.X) < Tolerance &&
                   Math.Abs(Y - other.Y) < Tolerance &&
                   Math.Abs(Z - other.Z) < Tolerance;
        }
        public int CompareTo(Vector3 other)
        {
            if (Equals(other)) return 0;
            if (X != other.X) return X.CompareTo(other.X);
            if (Y != other.Y) return Y.CompareTo(other.Y);
            return Z.CompareTo(other.Z);
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3 vector)
            {
                return Equals(vector);
            }
            return false;
        }
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
            info.AddValue("Z", Z);
        }
        public Vector3(SerializationInfo info, StreamingContext context)
        {
            X = info.GetDouble("X");
            Y = info.GetDouble("Y");
            Z = info.GetDouble("Z");
        }
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Vector3 operator *(Vector3 a, double scalar)
        {
            return new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        public static Vector3 operator /(Vector3 a, double scalar)
        {
            if (Math.Abs(scalar) < Tolerance)
                throw new DivideByZeroException("Cannot divide by zero.");
            return new Vector3(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !a.Equals(b);
        }
        public override string ToString()
        {
            return $"Vector3(X: {X}, Y: {Y}, Z: {Z})";
        }
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public static Vector3 Normalize(Vector3 vector)
        {
            double length = vector.Length();
            if (Math.Abs(length) < Tolerance)
                return vector;
            return vector / length;
        }
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }
        public static void Cross(Vector3 a, Vector3 b, out Vector3 result)
        {
            result = Cross(a, b);
        }
        public void Add(Vector3 other)
        {
            X += other.X;
            Y += other.Y;
            Z += other.Z;
        }
        public void Subtract(Vector3 other)
        {
            X -= other.X;
            Y -= other.Y;
            Z -= other.Z;
        }
        public void Multiply(double scalar)
        {
            X *= scalar;
            Y *= scalar;
            Z *= scalar;
        }
        public void Divide(double scalar)
        {
            if (Math.Abs(scalar) < Tolerance)
                throw new DivideByZeroException("Cannot divide by zero.");
            X /= scalar;
            Y /= scalar;
            Z /= scalar;
        }
        public static double Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        public Vector3 Rotate(double angle,Vector3 axis)
        {
            throw new NotImplementedException("Rotation not implemented yet.");
        }
        public void Reverse()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }
        public Vector3 Copy()
        {
            return new Vector3(this);
        }
        public Vector3 GlobalToLocal()
        {
            throw new NotImplementedException("Global to local transformation not implemented yet.");
        }
    }
}
