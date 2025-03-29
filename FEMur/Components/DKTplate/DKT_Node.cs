using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.Common;

namespace FEMur.Components.DKTplate
{
    public class DKT_Node : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Node()
          : base("Node", "N",
              "Node Component(DKTplate)",
              "FEMur", "DKTplate")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Point object", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> points = new List<GH_Point>();
            if (!DA.GetDataList(0, points)) return;

            List<INode> nodes = new List<INode>();
            for (int i = 0; i < points.Count; i++)
            {
                Point3d pt = points[i].Value;
                if (pt.IsValid)
                {
                    nodes.Add(new Node3D(i, pt.X, pt.Y, pt.Z));
                }
            }

            DA.SetDataList(0, nodes);
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
            get { return new Guid("00672C05-614F-4B04-A01F-4A546BEE505D"); }
        }
    }
}