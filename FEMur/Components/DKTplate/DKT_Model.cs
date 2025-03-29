using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.Common;

namespace FEMur.Components.DKTplate
{
    public class DKT_Model : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Model()
          : base("Model", "M",
              "FEMmodel component(DKTplate)",
              "FEMur", "DKTplate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Node object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Element", "E", "Element object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Support", "S", "Support object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load", "L", "Load object", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IElement> elements = new List<IElement>();
            List<INode> nodes = new List<INode>();
            List<ISupport> supports = new List<ISupport>();
            List<ILoad> loads = new List<ILoad>();
            int elementType = 1;

            if (!DA.GetDataList(1, elements)) return;
            if (!DA.GetDataList(0, nodes)) return;
            if (!DA.GetDataList(2, supports)) return;
            if (!DA.GetDataList(3, loads)) return;

            IFemModel model = new FemModel(nodes, elements, supports, loads);
            DA.SetData(0, model);
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
            get { return new Guid("6B416DA6-256C-4F96-BF41-1DEE8CD54E45"); }
        }
    }
}