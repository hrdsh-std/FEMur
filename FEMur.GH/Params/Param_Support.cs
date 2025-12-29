using System;
using Grasshopper.Kernel;
using FEMurGH.Goo;

namespace FEMurGH.Params
{
    /// <summary>
    /// Parameter for FEMur Support
    /// </summary>
    public class Param_Support : GH_Param<GH_Support>
    {
        public Param_Support()
            : base("FEMur Support", "Support",
                  "A boundary condition (support) for finite element analysis",
                  "FEMur", "1.Params",
                  GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("38DFA97A-B9AB-44B4-B109-48EE996F5441");

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Support;
    }
}