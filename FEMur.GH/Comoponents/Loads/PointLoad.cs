using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Loads;
using FEMur.Geometry;

namespace FEMurGH.Comoponents.Loads
{
    // GH コンポーネント: 入力 List<Point>, List<Vector3d>(Force), List<Vector3d>(Moment)
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
            pManager.AddVectorParameter("Force", "F", "Force vector [N] (Global coordinates). Single value applies to all points, or one per point", GH_ParamAccess.list);
            pManager.AddVectorParameter("Moment", "M", "Moment vector [N·mm] (Global coordinates). Single value applies to all points, or one per point", GH_ParamAccess.list);
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
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No points provided");
                return;
            }

            var ghForces = new List<Vector3d>();
            var ghMoments = new List<Vector3d>();

            if (!DA.GetDataList(1, ghForces))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No forces provided");
                return;
            }

            if (!DA.GetDataList(2, ghMoments))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No moments provided");
                return;
            }

            // 入力のバリデーション
            if (ghForces.Count == 0)
            {
                ghForces.Add(Vector3d.Zero);
            }

            if (ghMoments.Count == 0)
            {
                ghMoments.Add(Vector3d.Zero);
            }

            // リストのサイズチェック
            int pointCount = points.Count;
            int forceCount = ghForces.Count;
            int momentCount = ghMoments.Count;

            // 複数入力時のサイズ不一致チェック
            if (forceCount > 1 && forceCount != pointCount)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                    $"Force count ({forceCount}) must be 1 or match point count ({pointCount})");
                return;
            }

            if (momentCount > 1 && momentCount != pointCount)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, 
                    $"Moment count ({momentCount}) must be 1 or match point count ({pointCount})");
                return;
            }

            var loads = new List<FEMur.Loads.PointLoad>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                var pt = points[i];

                // Forceの取得（単一値の場合は全てに適用、複数の場合は対応するインデックス）
                Vector3d ghF = (forceCount == 1) ? ghForces[0] : ghForces[i];

                // Momentの取得（単一値の場合は全てに適用、複数の場合は対応するインデックス）
                Vector3d ghM = (momentCount == 1) ? ghMoments[0] : ghMoments[i];

                // Rhino Vector3d を FEMur Vector3 に変換
                var force = new Vector3(ghF.X, ghF.Y, ghF.Z);
                var moment = new Vector3(ghM.X, ghM.Y, ghM.Z);

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