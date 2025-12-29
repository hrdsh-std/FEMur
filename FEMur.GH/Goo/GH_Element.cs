using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using FEMur.Elements;

namespace FEMurGH.Goo
{
    /// <summary>
    /// Grasshopper wrapper for FEMur.Elements.ElementBase
    /// </summary>
    public class GH_Element : GH_Goo<ElementBase>
    {
        public GH_Element() { }

        public GH_Element(ElementBase element)
        {
            Value = element;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FEMur Element";

        public override string TypeDescription => "A finite element (beam, truss, etc.)";

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new GH_Element();
            return new GH_Element((ElementBase)Value.Clone());
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Element";
            return Value.ToString();
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
                return false;

            // ElementBaseまたはその派生クラスから直接キャスト
            if (source is ElementBase element)
            {
                Value = element;
                return true;
            }

            // GH_Elementからキャスト
            if (source is GH_Element ghElement)
            {
                Value = ghElement.Value;
                return true;
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (Value == null)
                return false;

            // ElementBaseへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(ElementBase)))
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

            // GH_Elementへキャスト
            if (typeof(Q).IsAssignableFrom(typeof(GH_Element)))
            {
                target = (Q)(object)this;
                return true;
            }

            return false;
        }
    }
}