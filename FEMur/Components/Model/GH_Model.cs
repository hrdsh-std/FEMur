using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Model;

namespace FEMur.Components.Model
{
    public class GH_Model : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Model()
          : base("Model", "M",
              "Model component",
              "FEMur", "Model")
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
            pManager.AddIntegerParameter("Element Type", "ET", "0:1st Order Element 1:Incompatible Element (Default) 2:2nd Order Element", GH_ParamAccess.item);
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
            List<Element> elements = new List<Element>();
            List<Node> nodes = new List<Node>();
            List<Support> supports = new List<Support>();
            List<Load> loads = new List<Load>();
            int elementType = 1;

            if(!DA.GetDataList(1, elements))return;
            if(!DA.GetDataList(0, nodes))return;
            if(!DA.GetDataList(2, supports))return;
            if(!DA.GetDataList(3, loads))return;
            if (!DA.GetData(4, ref elementType)) return;

            FEMur.Core.Model.FEMModel model = new FEMModel(nodes,elements,loads,supports,elementType);
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
            get { return new Guid("723CCE05-F95B-4A68-8121-8AD142B8ED77"); }
        }
    }
}