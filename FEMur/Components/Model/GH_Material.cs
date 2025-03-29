using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.FEMur2D.Model;

namespace FEMur.Components.Model
{
    public class GH_Material : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Material()
          : base("Material", "M",
              "Material component",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus of the material", GH_ParamAccess.item,205000);
            pManager.AddNumberParameter("Poisson's Ratio", "v", "Poisson's Ratio of the material", GH_ParamAccess.item,0.3);
            //pManager.AddNumberParameter("Density", "D", "Density of the material", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double E = 0;
            double nu = 0;
            //double density = 0;
            if (!DA.GetData(0, ref E)) return;
            if (!DA.GetData(1, ref nu)) return;
            //if (!DA.GetData(2, ref density)) return;
            Material material = new Material( E, nu);
            DA.SetData(0, material);
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
            get { return new Guid("BDEADE21-29E0-4EFD-AD9A-E6E61B5E9D7A"); }
        }
    }
}