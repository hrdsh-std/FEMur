using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Geometry
{
    public class Mesh3 :IEquatable<Mesh3>,IComparable<Mesh3>
    {
        public List<Point3> Vertices { get; set; }
        public List<Face3> Faces { get; set; }
        public Mesh3()
        {
            Vertices = new List<Point3>();
            Faces = new List<Face3>();
        }
        public Mesh3(List<Point3> vertices, List<Face3> faces)
        {
            Vertices = vertices;
            Faces = faces;
        }
        public bool Equals(Mesh3 other)
        {
            throw new NotImplementedException();
        }
        public int CompareTo(Mesh3 other)
        {
            throw new NotImplementedException();
        }

    }
}
