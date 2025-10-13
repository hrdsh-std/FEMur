using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Nodes;
using FEMur.Loads;
using FEMur.Geometry;

namespace FEMurGH.Loads
{
    // GH コンポーネント: 入力 List<Node>, Vector3d(Force), Vector3d(Moment), bool(isLocal)
    // 出力: List<FEMur.Loads.PointLoad>（Generic）
    public class PointLoad : GH_Component
    {
        public PointLoad()
          : base("PointLoad(FEMur)", "PointLoad",
              "Create FEMur PointLoads from Nodes, Force, Moment and Local flag",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "FEMur Nodes (List<FEMur.Nodes.Node>)", GH_ParamAccess.list);
            pManager.AddVectorParameter("Force", "F", "Force vector [N] (global or local depending on isLocal)", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddVectorParameter("Moment", "M", "Moment vector [N·mm] (global or local depending on isLocal)", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddBooleanParameter("isLocal", "L", "If true, vectors are defined in local coordinates", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // 出力は Generic として List<FEMur.Loads.PointLoad>
            pManager.AddGenericParameter("PointLoads", "PL", "FEMur PointLoads", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var nodes = new List<Node>();
            if (!DA.GetDataList(0, nodes) || nodes.Count == 0)
                return;

            Vector3d ghF = Vector3d.Zero;
            Vector3d ghM = Vector3d.Zero;
            bool isLocal = false;

            if (!DA.GetData(1, ref ghF)) return;
            if (!DA.GetData(2, ref ghM)) return;
            if (!DA.GetData(3, ref isLocal)) return;

            var force = new Vector3(ghF.X, ghF.Y, ghF.Z);
            var moment = new Vector3(ghM.X, ghM.Y, ghM.Z);

            var loads = new List<FEMur.Loads.PointLoad>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;
                loads.Add(new FEMur.Loads.PointLoad(n, force, moment, isLocal));
            }

            DA.SetDataList(0, loads);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("97ADB502-C65F-4B9C-AC02-559562BED095");
    }
}