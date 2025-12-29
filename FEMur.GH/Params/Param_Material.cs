using System;
using Grasshopper.Kernel;
using FEMurGH.Goo;

namespace FEMurGH.Params
{
    /// <summary>
    /// Parameter for FEMur Material
    /// </summary>
    public class Param_Material : GH_Param<GH_Material>
    {
        public Param_Material()
            : base("FEMur Material", "Material",
                  "A material definition for finite element analysis",
                  "FEMur", "1.Params",
                  GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("7C3E4F2A-8D5B-4E1C-9A6F-3B7D8C2E5A1F");

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Material;
    }
}
