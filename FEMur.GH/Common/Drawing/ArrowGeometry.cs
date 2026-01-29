using System.Drawing;
using Rhino.Geometry;

namespace FEMurGH.Common.Drawing
{
    /// <summary>
    /// —Í‚Ì–îˆóƒWƒIƒƒgƒŠ
    /// </summary>
    public class ArrowGeometry
    {
        public Point3d Start { get; set; }
        public Point3d End { get; set; }
        public Vector3d Direction { get; set; }
        public double Magnitude { get; set; }
        public Color Color { get; set; }
        public string Label { get; set; }
    }
}