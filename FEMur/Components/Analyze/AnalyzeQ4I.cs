using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Model;
using FEMur.Core.Analyze;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

namespace FEMur.Components.Analyze
{
    public class AnalyzeQ4I : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public AnalyzeQ4I()
          : base("Analyze", "A",
              "Analyze component",
              "FEMur", "Analyze")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddNumberParameter("d_ratio", "R", "deformation ratio", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddPointParameter("Deformation", "Deformation", "node displacement", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacement", "Disp", "displacement", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FEMModel model = null;
            double d_ratio = 0;
            DA.GetData(0, ref model);
            DA.GetData(1, ref d_ratio);
            SolverQ4I solver = new SolverQ4I(model);
            Matrix<double> d2d = solver.d;
            List<double> dtest = d2d.Enumerate().ToList();
            List<Point3d> newPoint = new List<Point3d>();
            for (int i = 0; i < model.nodes.Count; i++)
            {
                newPoint.Add(new Point3d(model.nodes[i].x + d2d[i * 2, 0] * d_ratio, model.nodes[i].y + d2d[i * 2 + 1, 0] * d_ratio, model.nodes[i].z));
            }
            DA.SetData(0, model);
            DA.SetDataList(1, newPoint);
            DA.SetDataList(2, dtest);

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
            get { return new Guid("C6511621-F8DB-440B-92EC-0A20D18E1095"); }
        }
    }
}