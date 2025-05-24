using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.DKTplate;
using FEMur.Core.Interface;
using FEMur.Core.FEMur2D.Model;
using System.Linq;
using System.Drawing;
using FEMur.Components.Result;

namespace FEMur.Components.DKTplate
{
    public class DKT_EigenMode : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DKT_EigenMode()
          : base("EigenMode", "EM",
              "EigenMode Component(DKTplate)",
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
            pManager.AddIntegerParameter("Mode", "M", "Mode number", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Deformation ratio", "DR", "deformation ratio", GH_ParamAccess.item, 50.0);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddGenericParameter("Result", "R", "Result object", GH_ParamAccess.item);
            pManager.AddMeshParameter("Deformed Mesh", "DM", "Deformed Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Eigen period(s)", "EP", "Eigen period", GH_ParamAccess.item);
            pManager.AddColourParameter("Legend Color", "C", "Legend Color", GH_ParamAccess.list);
            pManager.AddNumberParameter("Legend Tag", "T", "Legend Tag", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IFemModel model = null;
            FEMur.Core.DKTplate.Result result = null;
            int mode = 0;
            double d_ratio = 0;
            if (!DA.GetData(0, ref model))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
                return;
            }
            if (!DA.GetData(1, ref result))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
                return;
            }
            if (!DA.GetData(2, ref mode)) return;

            if (mode < 0 || mode >= result.eigenvalues.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mode number is out of range.");
                return;
            }

            if (!DA.GetData(3, ref d_ratio)) return;
            if (model == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
            }

            var eigenValue = result.eigenvalues[mode].Real;
            var omega = Math.Sqrt(eigenValue);
            var eigenperiod = (2 * Math.PI) / omega;
            var eigenVector = result.eigenvectors.Column(mode);

            Mesh deformation = deformationMesh(model, result, mode ,d_ratio);
            List<double> dNorm = Enumerable.Range(0, eigenVector.Count / 6).Select(i => Math.Sqrt(eigenVector[6 * i] * eigenVector[6 * i] + eigenVector[6 * i + 1] * eigenVector[6 * i + 1] + eigenVector[6 * i + 2] * eigenVector[6 * i + 2])).ToList();
            double max_d = dNorm.Max();

            //色の設定
            List<Color> colors = new List<Color>();
            colors = Util.GetColors(dNorm);

            List<double> tags = new List<double>();
            for (int i = 0; i < deformation.Vertices.Count; i++)
            {
                deformation.VertexColors.SetColor(i, colors[i]);
            }

            //出力するタグの処理
            List<double> tagList = new List<double>();
            List<Color> tagColor = new List<Color>();
            int COLORLEVEL = 10;
            for (int i = 0; i < COLORLEVEL; i++)
            {
                tagList.Add(dNorm.Min() + (dNorm.Max() - dNorm.Min()) / COLORLEVEL * i);
                tagColor.Add(Util.GetJetColor((double)i / (COLORLEVEL - 1)));
            }
            tagList.Reverse();
            tagColor.Reverse();

            DA.SetData(0, model);
            DA.SetData(1, result);
            DA.SetData(2, deformation);
            DA.SetData(3, eigenperiod);
            DA.SetDataList(4, tagColor);
            DA.SetDataList(5, tagList);
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
            get { return new Guid("2DFFACAE-AE0E-45A3-9220-E73CADB59A07"); }
        }

        public Mesh deformationMesh(IFemModel model, FEMur.Core.DKTplate.Result result,int mode , double d_ratio)
        {
            var eigenVector = result.eigenvectors.Column(mode);
            var eigenValue = result.eigenvalues[mode].Real;

            List<Point3d> deformation = new List<Point3d>();
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                deformation.Add(new Point3d(model.Nodes[i].X + eigenVector[i * 6] * d_ratio, model.Nodes[i].Y + eigenVector[i * 6 + 1] * d_ratio, model.Nodes[i].Z + eigenVector[i * 6 + 2] * d_ratio));
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