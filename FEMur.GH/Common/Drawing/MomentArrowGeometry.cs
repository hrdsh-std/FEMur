using System.Drawing;
using Rhino.Geometry;

namespace FEMurGH.Common.Drawing
{
    /// <summary>
    /// モーメントの矢印ジオメトリ
    /// </summary>
    public class MomentArrowGeometry
    {
        public Point3d Center { get; set; }
        public Vector3d Axis { get; set; }
        public double Radius { get; set; }
        public double Magnitude { get; set; }
        public bool Clockwise { get; set; }
        public Color Color { get; set; }
        public string Label { get; set; }
    }
}