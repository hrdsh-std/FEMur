using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Model;
using FEMur.Core.Analyze;

namespace FEMur.Components.Analyze
{
    public class AnalyzeQ4I : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public AnalyzeQ4I()
          : base("AnalyzeQ4Incomp", "AQ4I",
              "This component performs plane strain element analysis. Plane strain elements can only be analyzed in the XY plane.(Incompatible)",
              "FEMur", "Analyze")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "R", "Run analysis", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
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
            FEMModel model = null;
            bool run = false;
            if (!DA.GetData(0, ref model)) return;
            if (!DA.GetData(1, ref run)) return;
            if (!run) return;

            foreach (Node node in model.nodes)
            {
                if (node.z != 0)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plane strain element analysis can only be performed in the XY plane.");
                    return;
                }
            }

            Core.Analyze.AnalyzeQ4I solver = new Core.Analyze.AnalyzeQ4I(model);
            DA.SetData(0, solver.model);
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
            get { return new Guid("9427E647-5BCB-4753-8FE3-2C7ADC27359D"); }
        }
    }
}