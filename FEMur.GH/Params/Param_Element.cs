using System;
using Grasshopper.Kernel;
using FEMurGH.Goo;

namespace FEMurGH.Params
{
    /// <summary>
    /// Parameter for FEMur Element
    /// </summary>
    public class Param_Element : GH_Param<GH_Element>
    {
        public Param_Element()
            : base("FEMur Element", "Element",
                  "A finite element (beam, truss, etc.)",
                  "FEMur", "1.Params",
                  GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("CC655C7D-EE52-4F3F-96DF-9B86167E6A5A");

        protected override System.Drawing.Bitmap Icon => null;
    }
}