using System.Drawing;
using Rhino.Geometry;

namespace FEMurGH.Comoponents.Models
{
    /// <summary>
    /// ‹ÇŠÀ•WŒn–îˆó‚Ìî•ñ‚ğ•Û
    /// </summary>
    internal class LocalAxisArrow
    {
        public Point3d Start { get; set; }
        public Point3d End { get; set; }
        public Color Color { get; set; }
        public string Label { get; set; }
    }
}