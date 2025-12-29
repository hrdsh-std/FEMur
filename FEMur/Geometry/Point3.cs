using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Geometry
{
    public struct Point3 : IEquatable<Point3>, IComparable<Point3>
    {
        #region Properties
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
        public System.Numerics.Vector3 Vector3 => new System.Numerics.Vector3((float)X, (float)Y, (float)Z);

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

        public Point3(System.Numerics.Vector3 vector)
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

        /// <summary>
        /// 許容誤差を考慮して2つのPoint3が等しいかを判定
        /// </summary>
        /// <param name="other">比較対象のPoint3</param>
        /// <param name="tolerance">許容誤差</param>
        /// <returns>許容誤差以内で等しい場合はtrue</returns>
        public bool EqualsWithTolerance(Point3 other, double tolerance)
        {
            return Math.Abs(X - other.X) < tolerance &&
                   Math.Abs(Y - other.Y) < tolerance &&
                   Math.Abs(Z - other.Z) < tolerance;
        }

        /// <summary>
        /// 2点間の距離を計算
        /// </summary>
        /// <param name="other">もう一方の点</param>
        /// <returns>距離</returns>
        public double DistanceTo(Point3 other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public override bool Equals(object other)
        {
            if (other is Point3 point)
            {
                return X == point.X && Y == point.Y && Z == point.Z;
            }
            return false;
        }
        public override int GetHashCode() => Tuple.Create(X, Y, Z).GetHashCode();

        public int CompareTo(Point3 other)
        {
            if (X != other.X) return X.CompareTo(other.X);
            if (Y != other.Y) return Y.CompareTo(other.Y);
            return Z.CompareTo(other.Z);
        }
        public void Add(System.Numerics.Vector3 vec)
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

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        #endregion
    }

    /// <summary>
    /// Point3の座標比較器（許容誤差付き）
    /// Dictionary等のコレクションで許容誤差を考慮した等価性比較を行う
    /// </summary>
    public class Point3Comparer : IEqualityComparer<Point3>
    {
        private readonly double _tolerance;

        /// <summary>
        /// Point3Comparerを初期化
        /// </summary>
        /// <param name="tolerance">許容誤差（デフォルト: 0.001）</param>
        public Point3Comparer(double tolerance = 0.001)
        {
            _tolerance = tolerance;
        }

        /// <summary>
        /// 2つのPoint3が許容誤差以内で等しいかを判定
        /// </summary>
        public bool Equals(Point3 x, Point3 y)
        {
            return x.EqualsWithTolerance(y, _tolerance);
        }

        /// <summary>
        /// Point3のハッシュコードを計算（許容誤差を考慮）
        /// </summary>
        public int GetHashCode(Point3 obj)
        {
            // 許容誤差を考慮したハッシュコード生成
            // 座標を許容誤差でスケーリングして整数化
            int scale = (int)(1.0 / _tolerance);
            int hashX = ((int)(obj.X * scale)).GetHashCode();
            int hashY = ((int)(obj.Y * scale)).GetHashCode();
            int hashZ = ((int)(obj.Z * scale)).GetHashCode();
            return hashX ^ (hashY << 2) ^ (hashZ >> 2);
        }
    }
}
