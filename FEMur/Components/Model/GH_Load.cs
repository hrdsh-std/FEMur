using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Model;

namespace FEMur.Components.Model
{
    public class GH_Load : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Load()
          : base("Load", "Load",
              "Load",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Node object", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fx", "Fx", "Force in X direction", GH_ParamAccess.item,0.0);
            pManager.AddNumberParameter("Fy", "Fy", "Force in Y direction", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fz", "Fz", "Force in Z direction", GH_ParamAccess.item, 0.0);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
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
            List<Node> nodes = new List<Node>();
            double fx = 0;
            double fy = 0;
            double fz = 0;
            if(!DA.GetDataList(0, nodes)) return;
            if(!DA.GetData(1, ref fx))return;
            if(!DA.GetData(2, ref fy))return;
            if(!DA.GetData(3, ref fz))return;
            List<Load> loads = new List<Load>();
            foreach (Node node in nodes)
            {
                Load load = new Load(node, fx, fy, fz);
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
            get { return new Guid("04DECD58-5E5C-4647-ABBB-22FCA9223300"); }
        }
    }
}