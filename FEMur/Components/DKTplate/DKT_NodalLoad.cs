using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.Common;

namespace FEMur.Components.DKTplate
{
    public class DKT_NodalLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_NodalLoad()
          : base("NodalLoad", "NL",
              "NodalLoad Component",
              "FEMur", "DKTplate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Node object", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fx", "Fx", "Force in X direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fy", "Fy", "Force in Y direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fz", "Fz", "Force in Z direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Rx", "Rx", "Bending Moment in X direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Ry", "Ry", "Bending Moment in Y direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Rz", "Rz", "Bending Moment in Z direction", GH_ParamAccess.item, 0.0);
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
            pManager.AddGenericParameter("Load", "Load", "Load object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<INode> nodes = new List<INode>();
            double fx = 0;
            double fy = 0;
            double fz = 0;
            double rx = 0;
            double ry = 0;
            double rz = 0;
            if (!DA.GetDataList(0, nodes)) return;
            if (!DA.GetData(1, ref fx)) return;
            if (!DA.GetData(2, ref fy)) return;
            if (!DA.GetData(3, ref fz)) return;
            if (!DA.GetData(4, ref rx)) return;
            if (!DA.GetData(5, ref ry)) return;
            if (!DA.GetData(6, ref rz)) return;

            List<ILoad> loads = new List<ILoad>();
            foreach (INode node in nodes)
            {
                ILoad load = new NodalLoad(node.ID, fx, fy, fz, rx, ry, rz);
                loads.Add(load);
            }
            DA.SetDataList(0, loads);
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
            get { return new Guid("147B80B5-F841-43C9-83C4-68DEDA95C367"); }
        }
    }
}