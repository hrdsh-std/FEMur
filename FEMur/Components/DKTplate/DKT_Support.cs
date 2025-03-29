using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.Common;

namespace FEMur.Components.DKTplate
{
    public class DKT_Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Support()
          : base("Support", "S",
              "Support Component(DKTplate)",
              "FEMur", "DKTplate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Node object", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support UX", "UX", "Support in X direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Support UY", "UY", "Support in Y direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Support UZ", "UZ", "Support in Z direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Support RX", "RX", "Support in X direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Support RY", "RY", "Support in Y direction", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Support RZ", "RZ", "Support in Z direction", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
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
            List<INode> nodes = new List<INode>();
            bool ux = false;
            bool uy = false;
            bool uz = false;
            bool rx = false;
            bool ry = false;
            bool rz = false;

            if (!DA.GetDataList(0, nodes)) return;
            if (!DA.GetData(1, ref ux)) return;
            if (!DA.GetData(2, ref uy)) return;
            if (!DA.GetData(3, ref uz)) return;
            if (!DA.GetData(4, ref rx)) return;
            if (!DA.GetData(5, ref ry)) return;
            if (!DA.GetData(6, ref rz)) return;

            List<ISupport> supports = new List<ISupport>();
            foreach (INode node in nodes)
            {
                Support support = new Support(node.ID, ux, uy, uz, rx, ry, rz);
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
            get { return new Guid("3E6A8322-DAB9-45AC-97BA-439465AAB2FB"); }
        }
    }
}