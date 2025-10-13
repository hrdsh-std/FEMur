using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Nodes;

namespace FEMurGH.Supports
{
    public class Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support class.
        /// </summary>
        public Support()
          : base("Support(FEMur)", "Support",
              "Create FEMur Supports from Nodes with constraint conditions",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "FEMur Nodes (List<FEMur.Nodes.Node>)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("UX", "UX", "Fix translation in X direction", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("UY", "UY", "Fix translation in Y direction", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("UZ", "UZ", "Fix translation in Z direction", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("RX", "RX", "Fix rotation around X axis", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("RY", "RY", "Fix rotation around Y axis", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("RZ", "RZ", "Fix rotation around Z axis", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Supports", "S", "FEMur Supports", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var nodes = new List<Node>();
            if (!DA.GetDataList(0, nodes) || nodes.Count == 0)
                return;

            bool ux = false, uy = false, uz = false;
            bool rx = false, ry = false, rz = false;

            if (!DA.GetData(1, ref ux)) return;
            if (!DA.GetData(2, ref uy)) return;
            if (!DA.GetData(3, ref uz)) return;
            if (!DA.GetData(4, ref rx)) return;
            if (!DA.GetData(5, ref ry)) return;
            if (!DA.GetData(6, ref rz)) return;

            var supports = new List<FEMur.Supports.Support>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;
                
                // Support(int id, int nodeId, bool fixDX, bool fixDY, bool fixDZ, bool fixRX, bool fixRY, bool fixRZ)
                supports.Add(new FEMur.Supports.Support(i, n.Id, ux, uy, uz, rx, ry, rz));
            }

            DA.SetDataList(0, supports);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2A5B0B0D-58CD-4274-BBE0-F0466120B90D");
    }
}