using FEMur.Geometry;
using Rhino.Geometry;

namespace FEMurGH.Extensions
{
    /// <summary>
    /// FEMur.Geometry.Vector3 ÇÃägí£ÉÅÉ\ÉbÉh
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Rhino.Geometry.Vector3d Ç FEMur.Geometry.Vector3 Ç…ïœä∑
        /// </summary>
        /// <param name="vector">Rhino Vector3d</param>
        /// <returns>FEMur Vector3</returns>
        public static Vector3 ToFEMurVector3(this Vector3d vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// FEMur.Geometry.Vector3 Ç Rhino.Geometry.Vector3d Ç…ïœä∑
        /// </summary>
        /// <param name="vector">FEMur Vector3</param>
        /// <returns>Rhino Vector3d</returns>
        public static Vector3d ToRhinoVector3d(this Vector3 vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }
    }
}