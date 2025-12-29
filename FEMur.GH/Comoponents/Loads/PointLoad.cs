using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Loads;
using FEMur.Geometry;

namespace FEMurGH.Comoponents.Loads
{
    // GH コンポーネント: 入力 List<Point>, Vector3d(Force), Vector3d(Moment)
    // 出力: List<FEMur.Loads.PointLoad>（Generic）
    public class PointLoad : GH_Component
    {
        public PointLoad()
          : base("PointLoad(FEMur)", "PointLoad",
              "Create FEMur PointLoads from Points, Force, and Moment (Global coordinates)",
              "FEMur", "5.Load")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points where loads are applied (will match with existing nodes)", GH_ParamAccess.list);
            pManager.AddVectorParameter("Force", "F", "Force vector [N] (Global coordinates)", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddVectorParameter("Moment", "M", "Moment vector [N·mm] (Global coordinates)", GH_ParamAccess.item, Vector3d.Zero);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // 出力は Generic として List<FEMur.Loads.PointLoad>
            pManager.AddGenericParameter("PointLoads", "PL", "FEMur PointLoads", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var points = new List<Point3d>();
            if (!DA.GetDataList(0, points) || points.Count == 0)
                return;

            Vector3d ghF = Vector3d.Zero;
            Vector3d ghM = Vector3d.Zero;

            if (!DA.GetData(1, ref ghF)) return;
            if (!DA.GetData(2, ref ghM)) return;

            // Rhino Vector3d を FEMur Vector3 に変換
            var force = new Vector3(ghF.X, ghF.Y, ghF.Z);
            var moment = new Vector3(ghM.X, ghM.Y, ghM.Z);

            var loads = new List<FEMur.Loads.PointLoad>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                
                // Rhino Point3d を FEMur Point3 に変換
                var femurPoint = new Point3(pt.X, pt.Y, pt.Z);
                
                // PointLoad(Point3 position, Vector3 force, Vector3 moment)
                loads.Add(new FEMur.Loads.PointLoad(femurPoint, force, moment));
            }

            DA.SetDataList(0, loads);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("97ADB502-C65F-4B9C-AC02-559562BED095");
    }
}