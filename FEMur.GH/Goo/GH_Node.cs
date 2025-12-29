using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using FEMur.Nodes;

namespace FEMurGH.Goo
{
    /// <summary>
    /// Grasshopper wrapper for FEMur.Nodes.Node
    /// </summary>
    public class GH_Node : GH_Goo<Node>
    {
        public GH_Node() { }

        public GH_Node(Node node)
        {
            Value = node;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FEMur Node";

        public override string TypeDescription => "A finite element node with position and ID";

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Node();
            return new GH_Node(new Node(Value));
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Node";
            return $"Node(ID:{Value.Id}, Pos:{Value.Position})";
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;

            // FEMur.Nodes.Nodeから直接キャスト
            if (source is Node node)
            {
                Value = node;
                return true;
            }

            // GH_Nodeからキャスト
            if (source is GH_Node ghNode)
            {
                Value = ghNode.Value;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            // FEMur.Nodes.Nodeへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(Node)))
            {
                target = (Q)(object)Value;
                return true;
            }

            // GH_Nodeへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(GH_Node)))
            {
                target = (Q)(object)this;
                return true;
            }

            return false;
        }
    }
}