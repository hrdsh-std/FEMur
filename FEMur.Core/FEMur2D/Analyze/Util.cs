using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace FEMur.Core.FEMur2D.Analyze
{
    internal class Util
    {
        public static bool ConvertToXY(List<Point3d> points, out Plane plane, out Transform transform, out List<Point3d> transformedPoints, double tolerance = 1e-6)
        {
            transformedPoints = new List<Point3d>();

            // 平面フィッティング
            if (Plane.FitPlaneToPoints(points, out plane) != PlaneFitResult.Success)
            {
                transform = Transform.Identity;
                return false;
            }

            // 許容差をチェック
            foreach (Point3d p in points)
            {
                double distance = Math.Abs(plane.DistanceTo(p));
                if (distance > tolerance)
                {
                    transform = Transform.Identity;
                    return false;
                }
            }

            // 変換行列を作成
            transform = Transform.PlaneToPlane(plane, Plane.WorldXY);
            foreach (Point3d p in points)
            {
                p.Transform(transform);
                transformedPoints.Add(p);
            }

            return true;
        }
    }
}
