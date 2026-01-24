using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using FEMur.Materials;

namespace FEMurGH.Goo
{
    /// <summary>
    /// Grasshopper wrapper for FEMur.Materials.Material
    /// </summary>
    public class GH_Material : GH_Goo<Material>
    {
        public GH_Material() { }

        public GH_Material(Material material)
        {
            Value = material;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FEMur Material";

        public override string TypeDescription => "A material definition for finite element analysis";

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Material();
            return new GH_Material(Value);
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Material";
            return $"Material(Name:{Value.Name}, E:{Value.E})";
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;

            // FEMur.Materials.Materialから直接キャスト
            if (source is Material material)
            {
                Value = material;
                return true;
            }

            // GH_Materialからキャスト
            if (source is GH_Material ghMaterial)
            {
                Value = ghMaterial.Value;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            // FEMur.Materials.Materialへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(Material)))
            {
                target = (Q)(object)Value;
                return true;
            }

            // GH_Materialへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(GH_Material)))
            {
                target = (Q)(object)this;
                return true;
            }

            return false;
        }
    }
}
