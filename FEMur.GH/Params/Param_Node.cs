using System;
using Grasshopper.Kernel;
using FEMurGH.Goo;

namespace FEMurGH.Params
{
    /// <summary>
    /// Parameter for FEMur Node
    /// </summary>
    public class Param_Node : GH_Param<GH_Node>
    {
        public Param_Node()
            : base("FEMur Node", "Node",
                  "A finite element node with position and ID",
                  "FEMur", "1.Params",
                  GH_ParamAccess.item)
        {
        }

        public override Guid ComponentGuid => new Guid("7A558ADA-A72A-4387-985A-A6853C390802");

        protected override System.Drawing.Bitmap Icon => null;
    }
}