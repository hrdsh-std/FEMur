using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEMur.Core.Model
{
    public class GH_Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Support()
          : base("Support", "S",
              "Support component",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Node object", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support X", "UX", "Support in X direction", GH_ParamAccess.item,true);
            pManager.AddBooleanParameter("Support Y", "UY", "Support in Y direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Support Z", "UZ", "Support in Z direction", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "S", "Support object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Node> nodes = new List<Node>();
            bool ux = false;
            bool uy = false;
            bool uz = false;
            if(!DA.GetDataList(0, nodes)) return ;
            if (!DA.GetData(1, ref ux)) return;
            if (!DA.GetData(2, ref uy)) return;
            if (!DA.GetData(3, ref uz)) return;
            List<Support> supports = new List<Support>();
            foreach(Node node in nodes)
            {
                Support support = new Support(node, ux, uy, uz);
                supports.Add(support);
            }
            DA.SetDataList(0, supports);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1F2A98DF-A901-48A6-B5D2-80D3DEF39F26"); }
        }
    }
}