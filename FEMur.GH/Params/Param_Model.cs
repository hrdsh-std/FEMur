using System;
using Grasshopper.Kernel;
using FEMurGH.Goo;

namespace FEMurGH.Params
{
    /// <summary>
    /// Parameter for FEMur Model
    /// </summary>
    public class Param_Model : GH_Param<GH_Model>
    {
        public Param_Model()
            : base("FEMur Model", "Model",
                  "A finite element analysis model",
                  "FEMur", "1.Params",
                  GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("B321BE42-80A1-407A-997B-22A1A8143C31");

        protected override System.Drawing.Bitmap Icon => null;
    }
}