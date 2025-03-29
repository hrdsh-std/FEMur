using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using FEMur.Core.FEMur2D.Model;

namespace FEMur.Components.Model
{
    public class GH_Element4N : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Element4N()
          : base("Element4N", "E",
              "Element component for 4node elements.",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshFaceParameter("meshFace","MF", "meshFace", GH_ParamAccess.list);
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
            List<MeshFace> meshFaces = new List<MeshFace>();
            Material material = null;
            Section section = null;
            if (!DA.GetDataList(0, meshFaces)) return;
            if (!DA.GetData(1, ref material)) return;
            if (!DA.GetData(2, ref section)) return;

            List<Element> elements = new List<Element>();

            for(int i = 0; i < meshFaces.Count; i++)
            {
                List<int> nodes = new List<int> { meshFaces[i].A, meshFaces[i].B, meshFaces[i].C, meshFaces[i].D };
                Element element = new Element(i,nodes,material,section);
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
            get { return new Guid("849D2721-237E-41E7-8905-FF99EA314FFF"); }
        }
    }
}