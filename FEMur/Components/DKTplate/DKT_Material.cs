using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Interface;
using FEMur.Core.DKTplate;

namespace FEMur.Components.DKTplate
{
    public class DKT_Material : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Material()
          : base("Material", "M",
              "Material Component(DKTplate)",
              "FEMur", "DKTplate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus of the material(N/mm2)", GH_ParamAccess.item, 205000);
            pManager.AddNumberParameter("Poisson's Ratio", "v", "Poisson's Ratio of the material", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Mass Density", "D", "Mass Density of the material(g/cm3)", GH_ParamAccess.item, 7.85);
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
            double massDensity = 0.0;
            if (!DA.GetData(0, ref E)) return;
            if (!DA.GetData(1, ref nu)) return;
            if (!DA.GetData(2, ref massDensity)) return;

            //if (!DA.GetData(2, ref density)) return;
            IMaterial material = new Material(E, nu,massDensity);
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
            get { return new Guid("866F7D15-34F0-4509-B834-37220178EE07"); }
        }
    }
}