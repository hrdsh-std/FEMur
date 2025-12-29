using System;
using Grasshopper.Kernel;
using FEMurGH.Goo;

namespace FEMurGH.Params
{
    /// <summary>
    /// Parameter for FEMur Load
    /// </summary>
    public class Param_Load : GH_Param<GH_Load>
    {
        public Param_Load()
            : base("FEMur Load", "Load",
                  "A load (point load, element load, etc.) for finite element analysis",
                  "FEMur", "1.Params",
                  GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("1B6C83B3-B981-4DA4-AA8D-D560B2B44747");

        protected override System.Drawing.Bitmap Icon => null;
    }
}