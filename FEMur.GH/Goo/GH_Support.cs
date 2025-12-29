using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using FEMur.Supports;

namespace FEMurGH.Goo
{
    /// <summary>
    /// Grasshopper wrapper for FEMur.Supports.Support
    /// </summary>
    public class GH_Support : GH_Goo<Support>
    {
        public GH_Support() { }

        public GH_Support(Support support)
        {
            Value = support;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FEMur Support";

        public override string TypeDescription => "A boundary condition (support) for finite element analysis";

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Support();
            return new GH_Support((Support)Value.Clone());
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Support";

            string conditions = "";
            if (Value.Conditions != null && Value.Conditions.Length >= 6)
            {
                conditions = $"[{(Value.Conditions[0] ? "X" : "-")}" +
                           $"{(Value.Conditions[1] ? "Y" : "-")}" +
                           $"{(Value.Conditions[2] ? "Z" : "-")}" +
                           $"{(Value.Conditions[3] ? "Rx" : "--")}" +
                           $"{(Value.Conditions[4] ? "Ry" : "--")}" +
                           $"{(Value.Conditions[5] ? "Rz" : "--")}]";
            }

            return $"Support(Node:{Value.NodeId}, {conditions})";
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;

            // FEMur.Supports.Supportから直接キャスト
            if (source is Support support)
            {
                Value = support;
                return true;
            }

            // GH_Supportからキャスト
            if (source is GH_Support ghSupport)
            {
                Value = ghSupport.Value;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            // FEMur.Supports.Supportへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(Support)))
            {
                target = (Q)(object)Value;
                return true;
            }

            // GH_Supportへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(GH_Support)))
            {
                target = (Q)(object)this;
                return true;
            }

            return false;
        }
    }
}