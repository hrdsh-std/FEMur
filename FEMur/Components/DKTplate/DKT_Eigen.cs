using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.Common;

namespace FEMur.Components.DKTplate
{
    public class DKT_Eigen : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Eigen()
          : base("Eigen", "Eigen",
              "Eigen Component(DKTPlate)",
              "FEMur", "DKTplate")
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
            pManager.AddGenericParameter("Result", "R", "Result object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IFemModel model = null;
            Boolean run = false;
            if (!DA.GetData(0, ref model)) return;
            if (!DA.GetData(1, ref run)) return;
            if (!run) return;
            Eigen eigen = new Eigen(model);
            DA.SetData(0, eigen.model);
            DA.SetData(1, eigen.result);

            //List<Point3d> deformation = new List<Point3d>();
            //for (int i = 0; i < model.Nodes.Count; i++)
            //{
            //    deformation.Add(new Point3d(model.Nodes[i].X + eigenVector[i * 6] * d_ratio, model.Nodes[i].Y + eigenVector[i * 6 + 1] * d_ratio, model.Nodes[i].Z + eigenVector[i * 6 + 2] * d_ratio));
            //}
            //Mesh meshes = new Mesh();
            //foreach (Point3d p in deformation)
            //{
            //    meshes.Vertices.Add(p);
            //}
            //for (int i = 0; i < model.Elements.Count; i++)
            ////{
            ////    meshes.Faces.AddFace(model.Elements[i].NodesID[0], model.Elements[i].NodesID[1], model.Elements[i].NodesID[2]);
            ////}

            //DA.SetData(0, eigen.model);
            //DA.SetData(1, eigen.result.eigenvalues);
            //DA.SetData(2, meshes);
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
            get { return new Guid("4D69316C-8A9F-43F6-91F0-BA9FA218B56F"); }
        }
    }
}