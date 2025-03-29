using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.FEMur2D.Model;
using System.Linq;


namespace FEMur.Components.DKTplate
{
    public class DKT_Deformation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_Deformation()
          : base("Deformation", "D",
              "Defromation Component(DKTplate)",
              "FEMur", "DKTplate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddGenericParameter("Result", "R", "Result object", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation ratio", "DR", "deformation ratio", GH_ParamAccess.item, 50.0);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddGenericParameter("Result", "R", "Result object", GH_ParamAccess.item);
            pManager.AddMeshParameter("Deformation", "D", "Deformation", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Disp", "MaxD", "Max Displacement", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IFemModel model = null;
            FEMur.Core.DKTplate.Result result = null;
            double d_ratio = 0;
            if (!DA.GetData(0, ref model))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
                return;
            }
            if (!DA.GetData(1,ref result))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
                return;
            }
            if (!DA.GetData(2, ref d_ratio)) return;
            if (model == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
            }


            //明日ここから実装
            Mesh deformation = deformationMesh(model, result, d_ratio);
            var d = result.d;
            var dNorm = Enumerable.Range(0, d.RowCount /  6).Select(i => Math.Sqrt(d[6 * i, 0] * d[6 * i, 0] + d[6 * i + 1, 0] * d[6 * i + 1, 0] + d[6 * i + 2, 0] * d[6 * i + 2, 0])).ToArray();
            double max_d = dNorm.Max();

            DA.SetData(0, model);
            DA.SetData(1, result);
            DA.SetData(2, deformation);
            DA.SetData(3, max_d);
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
            get { return new Guid("B268ABAC-D5B8-4CFC-B853-3F8465C2C222"); }
        }

        public  Mesh deformationMesh(IFemModel model, FEMur.Core.DKTplate.Result result ,double d_ratio)
        {
            List<Point3d> deformation = new List<Point3d>();
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                deformation.Add(new Point3d(model.Nodes[i].X + result.d[i * 6, 0] * d_ratio, model.Nodes[i].Y + result.d[i * 6 + 1, 0] * d_ratio, model.Nodes[i].Z + result.d[i * 6 + 2, 0] * d_ratio));
            }
            //deformationをMeshに変換
            Mesh meshes = new Mesh();
            foreach (Point3d p in deformation)
            {
                meshes.Vertices.Add(p);
            }
            for (int i = 0; i < model.Elements.Count; i++)
            {
                meshes.Faces.AddFace(model.Elements[i].NodesID[0], model.Elements[i].NodesID[1], model.Elements[i].NodesID[2]);
            }
            return meshes;
        }
    }
}