using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Model;
using Grasshopper.Kernel.Types;

namespace FEMur.Components.Model
{
    public class GH_Node : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GH_Node()
          : base("Node", "Node",
            "Node Component. if the Z input is absent, it is considered a planar element.",
            "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Point object", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> points = new List<GH_Point>();
            if (!DA.GetDataList(0, points)) return;

            List<Node> nodes = new List<Node>();
            for (int i = 0; i < points.Count; i++)
            {
                Point3d pt = points[i].Value;
                if (pt.IsValid)
                {
                    if (pt.Z == 0)
                    {
                        nodes.Add(new Node(i, pt.X, pt.Y, pt.Z));
                    }
                    else
                    {
                        nodes.Add(new Node(i, pt.X, pt.Y, pt.Z));
                    }
                }
            }

            DA.SetDataList(0, nodes);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon@=> global::FEMur.Resource.Node;
        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a658ca11-1150-497a-b506-7a17dbaa635b");
    }
}