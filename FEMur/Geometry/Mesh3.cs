using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Geometry
{
    public class Mesh3 :IEquatable<Mesh3>,IComparable<Mesh3>
    {
        public List<Point3> Vertices { get; set; }
        public List<Face3> Faces { get; set; }
        public Guid Guid = Guid.NewGuid();
        public Mesh3()
        {
            Vertices = new List<Point3>();
            Faces = new List<Face3>();
        }
        public Mesh3(Mesh3 other)
        {
            Vertices = new List<Point3>(other.Vertices);
            Faces = new List<Face3>(other.Faces);
            Guid = other.Guid;
        }
        public Mesh3(List<Point3> vertices, List<Face3> faces)
        {
            Vertices = vertices;
            Faces = faces;
        }
        public bool Equals(Mesh3 other) => this.Guid == other.Guid;
        public int CompareTo(Mesh3 other)
        {
            throw new NotImplementedException();
        }
        public bool AddVertex(Point3 vertex)
        {
            Vertices.Add(vertex);
            return true;
        }
        public bool SetVertex(int index, Point3 vertex)
        {
            if (index < 0 || index >= Vertices.Count)
                throw new IndexOutOfRangeException("Index must be within the range of existing vertices.");
            Vertices[index] = vertex;
            return true;
        }
        public bool AddFace(Face3 face)
        {
            Faces.Add(face);
            return true;
        }

        public Mesh3 Copy() => new Mesh3(this);
        public Point3 GetVertex(int index) => Vertices[index];

        public Face3 GetFace(int index) => Faces[index];
        public Vector3 FaceNormal(Face3 face)
        {
            throw new NotImplementedException("Face normal calculation not implemented yet.");
        }

        public Vector3 FaceAreaVector(int v1, int v2, int v3)
        {
            throw new NotImplementedException("Face area vector calculation not implemented yet.");
        }
    }
}
