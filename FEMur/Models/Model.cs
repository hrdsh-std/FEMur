using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;
using FEMur.Geometry;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.BoundaryConditions;
using FEMur.Loads;

namespace FEMur.Models
{
    public class Model:CommonObject,ICloneable,ISerializable, IEquatable<Model>
    {
        public List<Node> Nodes { get; set; }
        public List<FEMur.Elements.ElementBase> Elements { get; set; }
        public List<Material> Materials { get; set; }
        public List<BoundaryCondition> BoundaryConditions { get; set; }
        public List<Load> Loads { get; set; }


    }
}
