using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.FEMur2D.Analyze;
using System.Linq;
using FEMur.Core.FEMur2D.Model;

namespace FEMur.Components.Result
{
    public class Deformation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Deformation()
          : base("Deformation", "D",
              "Deformation component",
              "FEMur", "Result")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation ratio", "DR", "deformation ratio", GH_ParamAccess.item,50.0);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddMeshParameter("Deformation", "D", "Deformation", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Disp","MaxD", "Max Displacement", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FEMModel model = null;
            double d_ratio = 0;
            if (!DA.GetData(0, ref model))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
                return;
            }
            if (!DA.GetData(1, ref d_ratio)) return;
            if (model == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
            }
            Mesh deformation = Util.deformationMesh(model, d_ratio);
            var d = model.result.d;
            var dNorm = Enumerable.Range(0,d.RowCount/2).Select(i => Math.Sqrt(d[2* i, 0]* d[2 * i, 0]+ d[2 * i+1, 0]* d[2 * i + 1, 0])).ToArray();
            double max_d = dNorm.Max();

            DA.SetData(0, model);
            DA.SetData(1, deformation);
            DA.SetData(2, max_d);
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
            get { return new Guid("54B1C4A8-156E-4BDF-8A06-FE7E4A992EF1"); }
        }
    }
}