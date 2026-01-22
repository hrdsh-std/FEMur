using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// 断面力図のプレビューデータ
    /// </summary>
    public class SectionForcePreview
    {
        public List<LineDiagram> Diagrams { get; set; } = new List<LineDiagram>();
        public List<FilledDiagram> FilledMeshes { get; set; } = new List<FilledDiagram>();
        public List<LabelDot> Labels { get; set; } = new List<LabelDot>();

        public static SectionForcePreview Empty => new SectionForcePreview();
    }

    /// <summary>
    /// 線図（ポリライン）
    /// </summary>
    public class LineDiagram
    {
        public Polyline Polyline { get; set; }
        public Color Color { get; set; }
        public int Thickness { get; set; } = 2;
    }

    /// <summary>
    /// 塗りつぶし図式（メッシュ）
    /// </summary>
    public class FilledDiagram
    {
        public Mesh Mesh { get; set; }
        public DisplayMaterial Material { get; set; }
        public Polyline Outline { get; set; }
        public Color OutlineColor { get; set; } = Color.Black;

        // i端とj端の断面力値を保存
        public double ValueAtI { get; set; }
        public double ValueAtJ { get; set; }
    }

    /// <summary>
    /// ラベルドット
    /// </summary>
    public class LabelDot
    {
        public Point3d Location { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public Color BgColor { get; set; }
    }
}