using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Models;
using FEMur.Elements;

namespace FEMurGH.Results
{
    public class SectionForceView : GH_Component
    {
        // 表示設定（UIから切り替え）
        public bool ShowFilled { get; set; } = false;
        public bool ShowNumbers { get; set; } = false;

        public enum SectionForceType
        {
            None,
            Fx,  // 軸力 (Nx)
            Fy,  // せん断力 Y
            Fz,  // せん断力 Z
            Mx,  // ねじりモーメント
            My,  // 曲げモーメント Y
            Mz   // 曲げモーメント Z
        }
        public SectionForceType SelectedForceType { get; set; } = SectionForceType.None;

        // 入力キャッシュ
        private Model _model;
        private double _scale = 1.0;

        // プレビューキャッシュ
        private SectionForcePreview _preview = SectionForcePreview.Empty;

        public SectionForceView()
          : base("SectionForceView(FEMur)", "SectionForceView",
              "Visualize section forces (axial, shear, moment) from analyzed model",
              "FEMur", "Results")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model with computed results", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "Display scale factor", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // 出力なし（ビューポート表示のみ）
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _model = null;
            _scale = 1.0;

            if (!DA.GetData(0, ref _model) || _model == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "AnalyzedModel is required");
                _preview = SectionForcePreview.Empty;
                return;
            }

            if (!DA.GetData(1, ref _scale))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Scale not set, using default 1.0");
                _scale = 1.0;
            }

            if (!_model.IsSolved || _model.Result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Model has not been solved yet. Run LinearStaticSolver first.");
                _preview = SectionForcePreview.Empty;
                return;
            }

            // 可視化データ生成
            _preview = SectionForceRenderer.BuildPreview(
                _model,
                _scale,
                SelectedForceType,
                ShowFilled,
                ShowNumbers
            );
        }

        public override void CreateAttributes()
        {
            m_attributes = new SectionForceViewAttributes(this);
        }

        // ビューポート描画
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_preview == null || (_preview.Diagrams.Count == 0 && _preview.Labels.Count == 0))
                return;

            var display = args.Display;

            // 図式ポリライン
            foreach (var line in _preview.Diagrams)
            {
                display.DrawPolyline(line.Polyline, line.Color, line.Thickness);
            }

            // 数値ラベル
            foreach (var lbl in _preview.Labels)
            {
                display.DrawDot(lbl.Location, lbl.Text, lbl.TextColor, lbl.BgColor);
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (_preview == null || _preview.FilledMeshes.Count == 0) return;

            var display = args.Display;
            foreach (var m in _preview.FilledMeshes)
            {
                display.DrawMeshShaded(m.Mesh, m.Material);
                // 境界線を少し強調
                if (m.Outline != null && m.Outline.Count > 1)
                {
                    display.DrawPolyline(m.Outline, m.OutlineColor, 1);
                }
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                if (_preview == null) return BoundingBox.Empty;
                BoundingBox bbox = BoundingBox.Empty;
                foreach (var l in _preview.Diagrams)
                    bbox.Union(l.Polyline.BoundingBox);
                foreach (var f in _preview.FilledMeshes)
                    bbox.Union(f.Mesh.GetBoundingBox(true));
                foreach (var d in _preview.Labels)
                    bbox.Union(new BoundingBox(d.Location, d.Location));
                return bbox;
            }
        }

        protected override Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("38F2E784-9423-41C5-BDB7-1F035C7E4241");
    }
}