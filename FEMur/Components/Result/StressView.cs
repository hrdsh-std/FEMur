using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Core.Model;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using System.Linq;
using Rhino.Geometry.Collections;

namespace FEMur.Components.Result
{
    public class StressView : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public StressView()
          : base("StressView", "SV",
              "StressView component",
              "FEMur", "Result")
        {
        }

        private Mesh meshes;
        private List<Curve> contourLines;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            //出力する成分を指定する入力
            pManager.AddIntegerParameter("StressType", "ST", "StressType", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Deformation ratio", "DR", "deformation ratio", GH_ParamAccess.item,50.0);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "Model object", GH_ParamAccess.item);
            pManager.AddColourParameter("Legend Color","C", "Legend Color", GH_ParamAccess.list);
            pManager.AddNumberParameter("Legend Tag", "T", "Legend Tag", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FEMModel model = null;
            int stressType = 0;
            double d_ratio = 0;
            DA.GetData(0, ref model);
            DA.GetData(1, ref stressType);
            DA.GetData(2, ref d_ratio);
            int COLORLEVEL = 10;
            if (model == null) 
            { 
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Analysis has not been executed.");
                return;
            };

            Matrix<double> stress = model.result.stress;

            List<double> stressList = new List<double>();
            for (int i = 0; i < model.nodes.Count; i++)
            {
                stressList.Add(stress[i, stressType]);
            }
            //描画するメッシュの処理
            meshes = Util.deformationMesh(model, d_ratio);
            List<Color> nodesColor = Util.GetColors(stressList);
            contourLines = Util.getContourLines(meshes, stressList);

            for (int i = 0; i < meshes.Vertices.Count; i++)
            {
                meshes.VertexColors.SetColor(i,nodesColor[i]);
            }

            ExpirePreview(true);

            //出力するタグの処理
            List<double> tagList = new List<double>();
            List<Color> tagColor = new List<Color>();
            for (int i = 0; i < COLORLEVEL; i++)
            {
                tagList.Add(stressList.Min() + (stressList.Max() - stressList.Min()) / COLORLEVEL * i);
                tagColor.Add(Util.GetJetColor((double)i / (COLORLEVEL - 1)));
            }
            tagList.Reverse();
            tagColor.Reverse();
            DA.SetData(0, model);
            DA.SetDataList(1, tagColor);
            DA.SetDataList(2, tagList);

            
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (meshes != null)
            {
                args.Display.DrawMeshFalseColors(meshes);
            }
        }
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if(meshes != null)
            {
                args.Display.DrawMeshWires(meshes, Color.DarkGray);
                foreach (Curve c in contourLines)
                {
                    args.Display.DrawCurve(c, Color.Gray);
                }
            }
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
            get { return new Guid("C332BFB6-074C-4A0B-A52E-F37A0AA4A110"); }
        }
    }
}