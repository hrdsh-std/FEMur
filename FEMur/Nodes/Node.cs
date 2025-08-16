using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FEMur.Geometry;

namespace FEMur.Nodes
{
    public class Node
    {
        #region Properties
        public int Id { get; }
        public Point3 Position { get; }
        public Point3 Position_disp { get; private set; }

        #endregion

        #region private Members

        #endregion

        #region Constructors
        public Node(int id, Point3 position)
        {
            Id = id;
            this.Position = position;
            this.Position_disp = position;
        }
        public Node(Node other)
        {
            this.Position = other.Position;
            this.Position_disp = other.Position_disp;
            this.Id = other.Id;
        }
        #endregion

        #region static members

        #endregion

        #region public methods
        public override string ToString()
        {
            return $"node-ind:{this.Id.ToString()}: ({this.Position.X.ToString()}/{this.Position.Y.ToString()}/{this.Position.Z.ToString()})";
        }

        #endregion

        #region private Methods

        #endregion

        #region operators

        #endregion
    }

}
