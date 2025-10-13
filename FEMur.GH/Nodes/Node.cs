using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Nodes;


namespace FEMurGH.Nodes
{
    /// <summary>
    /// 入力: List<Point>（Point3d） 出力: List<FEMur.Node>
    /// </summary>
    public class Nodes_FEMur_ : GH_Component
    {
        public Nodes_FEMur_()
          : base("Node(FEMur)", "Node",
              "Convert GH Points to FEMur Nodes (List<Point> -> List<FEMur.Node>)",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // 入力: GH の Point リスト
            pManager.AddPointParameter("Points", "P", "Node positions as Points (Rhino Point3d)", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // 出力: FEMur の Node リスト（Generic として出力）
            pManager.AddGenericParameter("Nodes", "N", "FEMur Nodes", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var points = new List<Point3d>();
            if (!DA.GetDataList(0, points)) return;

            var nodes = new List<Node>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                // Node(id, x, y, z) で作成（単位はモデルの前提に合わせて mm）
                nodes.Add(new Node(i, p.X, p.Y, p.Z));
            }

            DA.SetDataList(0, nodes);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("BD3663DC-7CDE-4137-8839-02416596FCE5");
    }
}