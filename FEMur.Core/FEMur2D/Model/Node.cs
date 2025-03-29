using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace FEMur.Core.FEMur2D.Model
{
    public class Node
    {
        public int id { get; }
        public Point3d point { get; set; }
        public Vector3d position { get; set; }
        public double x { get; }
        public double y { get; }
        public double z { get; }
        public List<double> Load { get; set; } = new List<double> { 0, 0, 0 };
        public List<bool> Constraint { get; set; } = new List<bool> { false, false, false };

        public Node(int id, double x, double y, double z)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.z = z;
            point = new Point3d(x, y, z);
        }
        public Node(int id, Point3d point)
        {
            this.id = id;
            this.point = point;
            x = point.X;
            y = point.Y;
            z = point.Z;
        }

        public override string ToString()
        {
            return $"Node {id}: x {x}, y {y}, z {z}";
        }
    }
}
