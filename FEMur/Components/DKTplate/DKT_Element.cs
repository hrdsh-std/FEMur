using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.DKTplate;
using FEMur.Core.Interface;

namespace FEMur.Components.DKTplate
{
    public class DKT_Element : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Element()
          : base("Element", "E",
              "Element Component(DKTplate)",
              "FEMur", "DKTplate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshFaceParameter("mesh", "mesh", "meshFace", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Material object", GH_ParamAccess.item);
            pManager.AddGenericParameter("Section", "S", "Section object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "E", "Element object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<MeshFace> mesh = new List<MeshFace>();
            IMaterial material = null;
            ISection section = null;
            if (!DA.GetDataList(0, mesh)) return;
            if (!DA.GetData(1, ref material)) return;
            if (!DA.GetData(2, ref section)) return;

            List<IElement> elements = new List<IElement>();

            for (int i = 0; i < mesh.Count; i++)
            {
                List<int> nodes = new List<int> { mesh[i].A, mesh[i].B, mesh[i].C };
                IElement element = new Element(i, nodes, material, section);
                elements.Add(element);
            }

            DA.SetDataList(0, elements);
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
            get { return new Guid("3677899A-55F9-498C-8A34-752D339A4F51"); }
        }
    }
}