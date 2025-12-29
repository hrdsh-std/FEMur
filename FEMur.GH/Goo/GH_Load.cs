using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using FEMur.Loads;

namespace FEMurGH.Goo
{
    /// <summary>
    /// Grasshopper wrapper for FEMur.Loads.Load
    /// </summary>
    public class GH_Load : GH_Goo<Load>
    {
        public GH_Load() { }

        public GH_Load(Load load)
        {
            Value = load;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FEMur Load";

        public override string TypeDescription => "A load (point load, element load, etc.) for finite element analysis";

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Load();
            return new GH_Load((Load)Value.Clone());
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Load";

            if (Value is PointLoad pointLoad)
            {
                return $"PointLoad(Node:{pointLoad.NodeId}, F:{pointLoad.Force})";
            }
            else if (Value is ElementLoad elementLoad)
            {
                return $"ElementLoad(Element:{elementLoad.ElementId}, Q:{elementLoad.QLocal})";
            }

            return $"Load({Value.GetType().Name})";
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;

            // Loadまたはその派生クラスから直接キャスト
            if (source is Load load)
            {
                Value = load;
                return true;
            }

            // GH_Loadからキャスト
            if (source is GH_Load ghLoad)
            {
                Value = ghLoad.Value;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            // Loadへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(Load)))
            {
                target = (Q)(object)Value;
                return true;
            }

            // 特定の派生型へのキャスト
            if (typeof(Q).IsAssignableFrom(Value.GetType()))
            {
                target = (Q)(object)Value;
                return true;
            }

            // GH_Loadへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(GH_Load)))
            {
                target = (Q)(object)this;
                return true;
            }

            return false;
        }
    }
}