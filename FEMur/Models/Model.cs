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
using FEMur.Supports;
using FEMur.Loads;

namespace FEMur.Models
{
    public class Model:CommonObject,ICloneable,ISerializable, IEquatable<Model>
    {
        public List<Node> Nodes { get; set; }
        public List<ElementBase> Elements { get; set; }
        public List<Support> Supports { get; set; }
        public List<Load> Loads { get; set; }

        public Model()
        {
            Nodes = new List<Node>();
            Elements = new List<ElementBase>();
            Supports = new List<Support>();
            Loads = new List<Load>();
        }
        public Model
            (List<Node> nodes, List<ElementBase> elements, 
            List<Support> supports,
            List<Load> loads)
        {
            Nodes = nodes;
            Elements = elements;
            Supports = supports;
            Loads = loads;
        }

        public Model(Model other)
        {
            this.Nodes = other.Nodes;
            this.Elements = other.Elements;
            this.Supports = other.Supports;
            this.Loads = other.Loads;
        }
        public Model(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Nodes = (List<Node>)info.GetValue("Nodes", typeof(List<Node>));
            Elements = (List<ElementBase>)info.GetValue("Elements", typeof(List<ElementBase>));
            Supports = (List<Support>)info.GetValue("Supports", typeof(List<Support>));
            Loads = (List<Load>)info.GetValue("Loads", typeof(List<Load>));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Nodes", Nodes);
            info.AddValue("Elements", Elements);
            info.AddValue("Supports", Supports);
            info.AddValue("Loads", Loads);
        }
        public override Object Clone() => this.MemberwiseClone();
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Model Summary:");
            sb.AppendLine($"Nodes: {Nodes.Count}");
            sb.AppendLine($"Elements: {Elements.Count}");
            sb.AppendLine($"Supports: {Supports.Count}");
            sb.AppendLine($"Loads: {Loads.Count}");
            return sb.ToString();
        }
        public bool Equals(Model other)
        {
            if (other == null) return false;
            return Nodes.SequenceEqual(other.Nodes) &&
                   Elements.SequenceEqual(other.Elements) &&
                   Supports.SequenceEqual(other.Supports) &&
                   Loads.SequenceEqual(other.Loads);
        }


    }
}
