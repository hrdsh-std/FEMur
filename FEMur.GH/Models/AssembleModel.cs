using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Supports;
using FEMur.Loads;
using FEMur.Models;

namespace FEMurGH.Models
{
    public class AssembleModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AssembleModel class.
        /// </summary>
        public AssembleModel()
          : base("AssembleModel(FEMur)", "AssembleModel",
              "Assemble FEMur Model from Nodes, Elements, Supports and Loads",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "FEMur Nodes (List<FEMur.Nodes.Node>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (List<FEMur.Elements.ElementBase>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "FEMur Supports (List<FEMur.Supports.Support>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "L", "FEMur Loads (List<FEMur.Loads.Load>)", GH_ParamAccess.list);
            
            // Supports と Loads はオプショナル（空リストでも解析可能）
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "FEMur Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var nodes = new List<Node>();
            var elements = new List<ElementBase>();
            var supports = new List<Support>();
            var loads = new List<Load>();

            if (!DA.GetDataList(0, nodes))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Nodes are required");
                return;
            }

            if (!DA.GetDataList(1, elements))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Elements are required");
                return;
            }

            // オプショナル入力
            DA.GetDataList(2, supports);
            DA.GetDataList(3, loads);

            // Model を作成
            var model = new Model(nodes, elements, supports, loads);

            DA.SetData(0, model);
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
            get { return new Guid("07CEABD2-D9DD-4C6A-ACFD-40EEFB58B622"); }
        }
    }
}