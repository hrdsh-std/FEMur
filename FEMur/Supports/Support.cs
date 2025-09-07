using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using FEMur.Utilities;
using FEMur.Nodes;

namespace FEMur.Supports
{
    public class Support: CommonObject, ICloneable,ISerializable
    {
        public int Id { get; }
        public int NodeId { get; }
        public double[] Displacement { get; private set; } = new double[6];
        public double[] Stiffness { get; private set; } = new double[6];
        public bool[] Condition { get; private set; } = new bool[6];
        private Node.DOF[] dofs = new Node.DOF[6];
        public Support() { }
        public Support(int id, int nodeId, double dx, double dy, double dz,double rx ,double ry,double rz)
        {
            Id = id;
            NodeId = nodeId;
            Displacement = new double[6] { dx, dy, dz, rx, ry, rz };
        }
        public Support(int id, Node node, double dx, double dy, double dz, double rx, double ry, double rz)
        {
            Id = id;
            NodeId = node.Id;
            Displacement = new double[6] { dx, dy, dz, rx, ry, rz };
        }
    }
}
