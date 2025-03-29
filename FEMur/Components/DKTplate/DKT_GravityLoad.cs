using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.Common;

namespace FEMur.Components.DKTplate
{
    public class DKT_GravityLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_GravityLoad()
          : base("GravityLoad", "GL",
              "GravityLoad Component",
              "FEMur", "DKTPlate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Gx", "Gx", "Gravity Coefficient in X direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Gy", "Gy", "Gravity Coefficient in Y direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Gz", "Gz", "Gravity Coefficient in Z direction", GH_ParamAccess.item, 0.0);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Load", "Load object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double gx = 0;
            double gy = 0;
            double gz = 0;
            if (!DA.GetData(0, ref gx)) return;
            if (!DA.GetData(1, ref gy)) return;
            if (!DA.GetData(2, ref gz)) return;
            
            ILoad load = new GravityLoad(gx, gy, gz);

            DA.SetData(0, load);
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
            get { return new Guid("DBBFE0EF-E7D2-45E7-836E-EAF393E4F60E"); }
        }
    }
}