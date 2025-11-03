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
        private double _autoScale = 1.0; // 自動計算されたスケール

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
            pManager.AddNumberParameter("Scale", "S", "Display scale factor (multiplied with auto-scale)", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // 出力なし（ビューポート表示のみ）
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _model = null;
            _scale = 1.0;
            _autoScale = 1.0;

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

            // モデルサイズに基づく自動スケール計算
            _autoScale = ComputeAutoScale(_model, SelectedForceType);

            // 最終スケール = 自動スケール × 入力スケール
            double finalScale = _autoScale * _scale;

            // 可視化データ生成
            _preview = SectionForceRenderer.BuildPreview(
                _model,
                finalScale,
                SelectedForceType,
                ShowFilled,
                ShowNumbers
            );
        }

        /// <summary>
        /// モデルの大きさと応力値に基づいて自動スケールを計算
        /// </summary>
        private double ComputeAutoScale(Model model, SectionForceType forceType)
        {
            if (model == null || model.Nodes == null || model.Nodes.Count == 0)
                return 1.0;

            if (model.Result == null || model.Result.ElementStresses == null || model.Result.ElementStresses.Count == 0)
                return 1.0;

            // 1. モデルの特性寸法を計算（バウンディングボックスの対角線長さ）
            double modelSize = CalculateModelCharacteristicLength(model);

            if (modelSize <= 0)
                return 1.0;

            // 2. 選択された断面力の最大値を取得
            double maxForce = GetMaxForceValue(model, forceType);

            if (maxForce <= 0)
                return 1.0;

            // 3. 自動スケール = (モデルサイズ × 0.1) / 最大断面力
            // 0.1は経験的な係数で、応力図がモデルサイズの約10%になるように調整
            double autoScale = (modelSize * 0.1) / maxForce;

            return autoScale;
        }

        /// <summary>
        /// モデルの特性寸法を計算（バウンディングボックスの対角線長さ）
        /// </summary>
        private double CalculateModelCharacteristicLength(Model model)
        {
            if (model.Nodes == null || model.Nodes.Count == 0)
                return 0.0;

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (var node in model.Nodes)
            {
                minX = Math.Min(minX, node.Position.X);
                minY = Math.Min(minY, node.Position.Y);
                minZ = Math.Min(minZ, node.Position.Z);
                maxX = Math.Max(maxX, node.Position.X);
                maxY = Math.Max(maxY, node.Position.Y);
                maxZ = Math.Max(maxZ, node.Position.Z);
            }

            double dx = maxX - minX;
            double dy = maxY - minY;
            double dz = maxZ - minZ;

            // バウンディングボックスの対角線長さ
            double diagonalLength = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            return diagonalLength;
        }

        /// <summary>
        /// 選択された断面力タイプの最大値を取得
        /// </summary>
        private double GetMaxForceValue(Model model, SectionForceType forceType)
        {
            if (model.Result == null || model.Result.ElementStresses == null)
                return 0.0;

            double maxValue = 0.0;

            foreach (var stress in model.Result.ElementStresses)
            {
                double value = 0.0;

                switch (forceType)
                {
                    case SectionForceType.Fx:
                        value = Math.Max(Math.Abs(stress.Fx_i), Math.Abs(stress.Fx_j));
                        break;
                    case SectionForceType.Fy:
                        value = Math.Max(Math.Abs(stress.Fy_i), Math.Abs(stress.Fy_j));
                        break;
                    case SectionForceType.Fz:
                        value = Math.Max(Math.Abs(stress.Fz_i), Math.Abs(stress.Fz_j));
                        break;
                    case SectionForceType.Mx:
                        value = Math.Max(Math.Abs(stress.Mx_i), Math.Abs(stress.Mx_j));
                        break;
                    case SectionForceType.My:
                        value = Math.Max(Math.Abs(stress.My_i), Math.Abs(stress.My_j));
                        break;
                    case SectionForceType.Mz:
                        value = Math.Max(Math.Abs(stress.Mz_i), Math.Abs(stress.Mz_j));
                        break;
                    case SectionForceType.None:
                    default:
                        continue;
                }

                maxValue = Math.Max(maxValue, value);
            }

            return maxValue;
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