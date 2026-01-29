using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Loads;
using FEMur.Geometry;
using FEMurGH.Extensions;

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
            
            // ForceとMomentはオプション入力にし、デフォルト値をゼロベクトルに設定
            pManager[1].Optional = true;
            pManager[2].Optional = true;
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

            // Forceの取得（入力がない場合はゼロベクトル）
            if (!DA.GetDataList(1, ghForces) || ghForces.Count == 0)
            {
                ghForces.Add(Vector3d.Zero);
            }

            // Momentの取得（入力がない場合はゼロベクトル）
            if (!DA.GetDataList(2, ghMoments) || ghMoments.Count == 0)
            {
                ghMoments.Add(Vector3d.Zero);
            }

            // リストのサイズを取得
            int pointCount = points.Count;
            int forceCount = ghForces.Count;
            int momentCount = ghMoments.Count;

            // エラーチェック: Points と Force が両方とも複数で、かつ個数が一致しない場合
            if (pointCount > 1 && forceCount > 1 && pointCount != forceCount)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Point count ({pointCount}) and Force count ({forceCount}) must match when both are greater than 1");
                return;
            }

            // エラーチェック: Points と Moment が両方とも複数で、かつ個数が一致しない場合
            if (pointCount > 1 && momentCount > 1 && pointCount != momentCount)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Point count ({pointCount}) and Moment count ({momentCount}) must match when both are greater than 1");
                return;
            }

            // 最大のカウント数を取得（Grasshopperのデータマッチング動作を再現）
            int maxCount = Math.Max(pointCount, Math.Max(forceCount, momentCount));

            List<FEMur.Loads.PointLoad> loads = new List<FEMur.Loads.PointLoad>(maxCount);

            for (int i = 0; i < maxCount; i++)
            {
                // Grasshopperのデータマッチング: 最後の要素を繰り返す
                var pt = points[Math.Min(i, pointCount - 1)];
                Vector3d ghF = ghForces[Math.Min(i, forceCount - 1)];
                Vector3d ghM = ghMoments[Math.Min(i, momentCount - 1)];

                // Rhino型をFEMur型に変換（拡張メソッドを使用）
                var femurPoint = pt.ToFEMurPoint3();
                var force = ghF.ToFEMurVector3();
                var moment = ghM.ToFEMurVector3();

                loads.Add(new FEMur.Loads.PointLoad(femurPoint, force, moment));
            }

            DA.SetDataList(0, loads);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("97ADB502-C65F-4B9C-AC02-559562BED095");
    }
}