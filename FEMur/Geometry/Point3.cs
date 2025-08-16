using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Geometry
{
    public struct Point3 : IEquatable<Point3>,IComparable<Point3>
    {
        #region Properties
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
        public Vector3 Vector3 => new Vector3((float)X, (float)Y, (float)Z);

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.X = value;
                        break;
                    case 1:
                        this.Y = value;
                        break;
                    case 2:
                        this.Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        #region static members

        #endregion

        #region Constructors

        public Point3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(Point3 other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public Point3(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        #endregion

        #region public methods
        public bool Equals(Point3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        public override bool Equals(object other)
        {
            if (other is Point3 point)
            {
                return X == point.X && Y == point.Y && Z == point.Z;
            }
            return false;
        }
        public override int GetHashCode() => Tuple.Create(X,Y,Z).GetHashCode();


        public int CompareTo(Point3 other)
        {
            if (X != other.X) return X.CompareTo(other.X);
            if (Y != other.Y) return Y.CompareTo(other.Y);
            return Z.CompareTo(other.Z);
        }
        public void Add(Vector3 vec)
        {
            this.X += vec.X;
            this.Y += vec.Y;
            this.Z += vec.Z;
        }
        #endregion

        #region private Members

        #endregion

        #region private Methods

        #endregion

        #region operators

        public static Point3 operator +(Point3 a, Point3 b)
        {
            return new Point3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Point3 operator -(Point3 a, Point3 b)
        {
            return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Point3 operator *(Point3 a, double scalar)
        {
            return new Point3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        public static Point3 operator *(double scalar, Point3 a)
        {
            return new Point3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        public static Point3 operator /(Point3 a, double scalar)
        {
            if (scalar == 0) throw new DivideByZeroException();
            return new Point3(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }
        public static Point3 operator /(double scalar, Point3 a)
        {
            if (a.X == 0 || a.Y == 0 || a.Z == 0) throw new DivideByZeroException();
            return new Point3(scalar / a.X, scalar / a.Y, scalar / a.Z);
        }
        public static bool operator ==(Point3 a, Point3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        }
        public static bool operator !=(Point3 a, Point3 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }


        #endregion
    }
}
