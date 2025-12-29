using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using FEMur.Models;

namespace FEMurGH.Goo
{
    /// <summary>
    /// Grasshopper wrapper for FEMur.Models.Model
    /// </summary>
    public class GH_Model : GH_Goo<Model>
    {
        public GH_Model() { }

        public GH_Model(Model model)
        {
            Value = model;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FEMur Model";

        public override string TypeDescription => "A finite element analysis model";

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Model();
            return new GH_Model((Model)Value.Clone());
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Model";
            return Value.ToString();
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;

            // FEMur.Models.Modelから直接キャスト
            if (source is Model model)
            {
                Value = model;
                return true;
            }

            // GH_Modelからキャスト
            if (source is GH_Model ghModel)
            {
                Value = ghModel.Value;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            // FEMur.Models.Modelへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(Model)))
            {
                target = (Q)(object)Value;
                return true;
            }

            // GH_Modelへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(GH_Model)))
            {
                target = (Q)(object)this;
                return true;
            }

            return false;
        }
    }
}