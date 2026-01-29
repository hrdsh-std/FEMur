using FEMur.Geometry;
using Rhino.Geometry;

namespace FEMurGH.Extensions
{
    /// <summary>
    /// FEMur.Geometry.Point3 ‚ÌŠg’£ƒƒ\ƒbƒh
    /// </summary>
    public static class Point3Extensions
    {
        /// <summary>
        /// Rhino.Geometry.Point3d ‚ğ FEMur.Geometry.Point3 ‚É•ÏŠ·
        /// </summary>
        /// <param name="point">Rhino Point3d</param>
        /// <returns>FEMur Point3</returns>
        public static Point3 ToFEMurPoint3(this Point3d point)
        {
            return new Point3(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// FEMur.Geometry.Point3 ‚ğ Rhino.Geometry.Point3d ‚É•ÏŠ·
        /// </summary>
        /// <param name="point">FEMur Point3</param>
        /// <returns>Rhino Point3d</returns>
        public static Point3d ToRhinoPoint3d(this Point3 point)
        {
            return new Point3d(point.X, point.Y, point.Z);
        }
    }
}