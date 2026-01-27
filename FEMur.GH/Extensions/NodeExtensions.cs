using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Nodes;
using FEMur.Geometry;
using Rhino.Geometry;
using System.Runtime.CompilerServices;

namespace FEMurGH.Extensions
{
    public static class NodeExtensions
    {
        public static Point3d ToRhinoPoint(this Node node)
        {
            return new Point3d (
                node.Position.X,
                node.Position.Y,
                node.Position.Z
            );
        }
    }
}
