using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Models;
using FEMur.Elements;
using FEMurGH.Comoponents.Results;

namespace FEMurGH.Comoponents.Results
{
    public class SectionForceView : GH_Component
    {
        // 表示設定（UIから切り替え）
        public bool ShowFilled { get; set; } = false;
        public bool ShowNumbers { get; set; } = false;
        public bool ShowLegend { get; set; } = true; // Legendスイッチを追加
        public SectionForceType SelectedForceType { get; set; } = SectionForceType.None;

        // 展開タブの状態
        public bool IsSectionForcesTabExpanded { get; set; } = false;

        // 単位設定
        public ForceUnit SelectedForceUnit { get; set; } = ForceUnit.N;
        public LengthUnit SelectedLengthUnit { get; set; } = LengthUnit.mm;

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

        public enum ForceUnit
        {
            N,   // ニュートン
            kN   // キロニュートン
        }

        public enum LengthUnit
        {
            mm,  // ミリメートル
            m    // メートル
        }

        // 入力キャッシュ
        private Model _model;
        private double _scale = 1.0;
        private double _autoScale = 1.0; // 自動計算されたスケール

        // プレビューキャッシュ
        private SectionForcePreview _preview = SectionForcePreview.Empty;

        public SectionForceView()
          : base("SectionForceView(FEMur)", "SectionForceView",
              "Visualize section forces (axial, shear, moment) from analyzed model",
              "FEMur", "9.Result")
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
                ShowNumbers,
                this  // thisを追加
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

            // 2. 選択された断面力の最大値と最小値を取得
            GetMinMaxForceValue(model, forceType, out double minForce, out double maxForce);

            double maxAbsForce = Math.Max(Math.Abs(minForce), Math.Abs(maxForce));

            if (maxAbsForce <= 0)
                return 1.0;

            // 3. 自動スケール = (モデルサイズ × 0.1) / 最大断面力の絶対値
            // 応力図がモデルサイズの約10%になるように調整
            double autoScale = (modelSize * 0.1) / maxAbsForce;

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
        /// 選択された断面力タイプの最小値と最大値を取得
        /// </summary>
        private void GetMinMaxForceValue(Model model, SectionForceType forceType, out double minValue, out double maxValue)
        {
            minValue = 0.0;
            maxValue = 0.0;

            if (model.Result == null || model.Result.ElementStresses == null)
                return;

            bool firstValue = true;

            foreach (var stress in model.Result.ElementStresses)
            {
                double value_i = 0.0;
                double value_j = 0.0;

                switch (forceType)
                {
                    case SectionForceType.Fx:
                        value_i = stress.Fx_i;
                        value_j = stress.Fx_j;
                        break;
                    case SectionForceType.Fy:
                        value_i = stress.Fy_i;
                        value_j = stress.Fy_j;
                        break;
                    case SectionForceType.Fz:
                        value_i = stress.Fz_i;
                        value_j = stress.Fz_j;
                        break;
                    case SectionForceType.Mx:
                        value_i = stress.Mx_i;
                        value_j = stress.Mx_j;
                        break;
                    case SectionForceType.My:
                        value_i = stress.My_i;
                        value_j = stress.My_j;
                        break;
                    case SectionForceType.Mz:
                        value_i = stress.Mz_i;
                        value_j = stress.Mz_j;
                        break;
                    case SectionForceType.None:
                    default:
                        continue;
                }

                if (firstValue)
                {
                    minValue = Math.Min(value_i, value_j);
                    maxValue = Math.Max(value_i, value_j);
                    firstValue = false;
                }
                else
                {
                    minValue = Math.Min(minValue, Math.Min(value_i, value_j));
                    maxValue = Math.Max(maxValue, Math.Max(value_i, value_j));
                }
            }
        }

        /// <summary>
        /// 選択された断面力タイプの最大絶対値を取得（後方互換性のため残す）
        /// </summary>
        private double GetMaxForceValue(Model model, SectionForceType forceType)
        {
            GetMinMaxForceValue(model, forceType, out double minValue, out double maxValue);
            return Math.Max(Math.Abs(minValue), Math.Abs(maxValue));
        }

        /// <summary>
        /// 値を色に変換（コンターマッピング）
        /// </summary>
        /// <param name="value">断面力の値（符号付き）</param>
        /// <param name="minValue">断面力の最小値</param>
        /// <param name="maxValue">断面力の最大値</param>
        /// <returns>コンター色</returns>
        private Color GetColor(double value, double minValue, double maxValue)
        {
            double range = maxValue - minValue;
            
            // レンジが非常に小さい場合は単色（緑）で表示
            // 最大絶対値の1%未満の変動は無視
            double maxAbs = Math.Max(Math.Abs(minValue), Math.Abs(maxValue));
            double threshold = maxAbs * 0.01; // 1%の閾値
            
            // 絶対値が非常に小さい場合も考慮（例：1e-6未満）
            if (maxAbs < 1e-6)
            {
                threshold = 1e-6;
            }
            
            if (range < threshold)
            {
                // ほぼ同じ値の場合は中間色（緑）で表示
                return Color.FromArgb(255, 0, 255, 0);
            }
            
            // 通常のカラーマッピング
            double t = (value - minValue) / range;
            t = Math.Max(0.0, Math.Min(1.0, t));

            double r = 0, g = 0, b = 0;

            if (t < 0.25)
            {
                // Blue -> Cyan
                r = 0;
                g = 4 * t;
                b = 1;
            }
            else if (t < 0.5)
            {
                // Cyan -> Green
                r = 0;
                g = 1;
                b = 1 + 4 * (0.25 - t);
            }
            else if (t < 0.75)
            {
                // Green -> Yellow
                r = 4 * (t - 0.5);
                g = 1;
                b = 0;
            }
            else
            {
                // Yellow -> Red
                r = 1;
                g = 1 + 4 * (0.75 - t);
                b = 0;
            }

            return Color.FromArgb(
                255,
                (int)(255 * r),
                (int)(255 * g),
                (int)(255 * b)
            );
        }

        public override void CreateAttributes()
        {
            m_attributes = new SectionForceViewAttributes(this);
        }

        // ビューポート描画
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            // プレビューが空でも、Legendを表示する必要があるため条件を変更
            if (_preview == null)
                return;

            var display = args.Display;

            // 図式ポリライン
            foreach (var line in _preview.Diagrams)
            {
                display.DrawPolyline(line.Polyline, line.Color, line.Thickness);
            }

            // 数値ラベル（背景なし、黒文字で3D空間に描画）
            foreach (var lbl in _preview.Labels)
            {
                // DrawDotを使用（背景色を半透明にして目立たないようにする）
                Color dotColor = Color.Transparent;
                Color textColor = Color.Black;
                display.Draw2dText(lbl.Text, lbl.TextColor, lbl.Location, true);
            }

            // 凡例を描画（ShowLegendがtrueの場合のみ）
            if (ShowLegend && SelectedForceType != SectionForceType.None && _model != null)
            {
                DrawLegend(args);
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (_preview == null || _preview.FilledMeshes.Count == 0) return;

            var display = args.Display;

            foreach (var diagram in _preview.FilledMeshes)
            {
                if (diagram.Mesh == null || diagram.Mesh.Vertices.Count == 0)
                    continue;

                // メッシュは既に頂点カラーを持っているので、そのまま描画
                display.DrawMeshShaded(diagram.Mesh, new Rhino.Display.DisplayMaterial(Color.White));

                // 境界線を少し強調
                if (diagram.Outline != null && diagram.Outline.Count > 1)
                {
                    display.DrawPolyline(diagram.Outline, diagram.OutlineColor, 1);
                }
            }
        }

        /// <summary>
        /// ビューポート右側に凡例を描画
        /// </summary>
        private void DrawLegend(IGH_PreviewArgs args)
        {
            var display = args.Display;
            var viewport = args.Viewport;

            // ビューポートのサイズを取得
            int viewportWidth = viewport.Size.Width;
            int viewportHeight = viewport.Size.Height;

            // 凡例の設定
            int colorBarWidth = 40;
            int margin = 40;
            int topMargin = 50;
            int bottomMargin = 50;
            
            // colorBarHeightをviewportHeightから計算（上下のマージンを引く）
            int colorBarHeight = viewportHeight - topMargin - bottomMargin;

            // 右側に配置
            int colorBarX = viewportWidth - colorBarWidth - margin;
            int colorBarY = topMargin;

            // タイトルを描画（カラーバーの上）
            string title = GetForceTypeLabel(SelectedForceType);
            string unit = GetForceTypeUnit(SelectedForceType);
            display.Draw2dText($"{title} [{unit}]", Color.Black, new Point2d(colorBarX, colorBarY - 20), false, 12);

            // 最小値・最大値を取得
            GetMinMaxForceValue(_model, SelectedForceType, out double minValue, out double maxValue);

            // 単位変換
            bool isMoment = (SelectedForceType == SectionForceType.Mx ||
                            SelectedForceType == SectionForceType.My ||
                            SelectedForceType == SectionForceType.Mz);
            double displayMin = ConvertSectionForceValue(minValue, isMoment);
            double displayMax = ConvertSectionForceValue(maxValue, isMoment);

            // レンジチェック
            double range = maxValue - minValue;
            double maxAbs = Math.Max(Math.Abs(minValue), Math.Abs(maxValue));
            double threshold = maxAbs * 0.01;
            if (maxAbs < 1e-6)
            {
                threshold = 1e-6;
            }
            bool isUniformColor = (range < threshold);

            // カラーバーを分割して描画
            int segments = 50;
            
            if (isUniformColor)
            {
                // 単色（緑）で塗りつぶし
                Color uniformColor = Color.FromArgb(255, 0, 255, 0);
                Rectangle rect = new Rectangle(colorBarX, colorBarY, colorBarWidth, colorBarHeight);
                display.Draw2dRectangle(rect, uniformColor, 0, uniformColor);
            }
            else
            {
                // 通常のグラデーション表示（上から下へ：最大値→最小値）
                for (int i = 0; i < segments; i++)
                {
                    // tを反転：0（上）が最大値、1（下）が最小値になるように
                    double t = 1.0 - (double)i / segments;
                    double value = minValue + (maxValue - minValue) * t;
                    Color color = GetColor(value, minValue, maxValue);

                    // 上から下へ描画
                    int y = colorBarY + (int)((double)i / segments * colorBarHeight);
                    int h = (int)Math.Ceiling((double)colorBarHeight / segments) + 1; // 隙間を防ぐため+1

                    Rectangle rect = new Rectangle(colorBarX, y, colorBarWidth, h);
                    display.Draw2dRectangle(rect, color, 0, color);
                }
            }

            // カラーバーの枠線
            display.Draw2dRectangle(new Rectangle(colorBarX, colorBarY, colorBarWidth, colorBarHeight), Color.Black, 2, Color.Transparent);

            if (isUniformColor)
            {
                // 単色の場合は中央に1つの値だけ表示
                double displayValue = ConvertSectionForceValue((minValue + maxValue) / 2, isMoment);
                display.Draw2dText($"{displayValue:F2}", Color.Black, new Point2d(colorBarX + colorBarWidth + 5, colorBarY + colorBarHeight / 2 - 5), false, 10);
            }
            else
            {
                // 通常のラベル表示
                // 最大値・最小値のラベル（カラーバーの右側）
                display.Draw2dText($"{displayMax:F2}", Color.Black, new Point2d(colorBarX + colorBarWidth + 5, colorBarY), false, 10);
                display.Draw2dText($"{displayMin:F2}", Color.Black, new Point2d(colorBarX + colorBarWidth + 5, colorBarY + colorBarHeight - 10), false, 10);
                
                // 中間値のラベルも追加（オプション）
                double displayMid = ConvertSectionForceValue((minValue + maxValue) / 2, isMoment);
                display.Draw2dText($"{displayMid:F2}", Color.Black, new Point2d(colorBarX + colorBarWidth + 5, colorBarY + colorBarHeight / 2 - 5), false, 10);

                // 1/4値のラベルも追加（オプション）
                double displayQuarter1 = ConvertSectionForceValue(minValue + (maxValue - minValue) * 0.25, isMoment);
                display.Draw2dText($"{displayQuarter1:F2}", Color.Black, new Point2d(colorBarX + colorBarWidth + 5, colorBarY + (colorBarHeight * 3 / 4) - 5), false, 10);

                // 3/4値のラベルも追加（オプション）
                double displayQuarter3 = ConvertSectionForceValue(minValue + (maxValue - minValue) * 0.75, isMoment);
                display.Draw2dText($"{displayQuarter3:F2}", Color.Black, new Point2d(colorBarX + colorBarWidth + 5, colorBarY + colorBarHeight / 4 - 5), false, 10);
            }
        }

        /// <summary>
        /// 断面力タイプのラベルを取得
        /// </summary>
        private string GetForceTypeLabel(SectionForceType forceType)
        {
            switch (forceType)
            {
                case SectionForceType.Fx: return "N";
                case SectionForceType.Fy: return "Qy";
                case SectionForceType.Fz: return "Qz";
                case SectionForceType.Mx: return "Mx";
                case SectionForceType.My: return "My";
                case SectionForceType.Mz: return "Mz";
                default: return "";
            }
        }

        /// <summary>
        /// 断面力タイプの単位を取得
        /// </summary>
        private string GetForceTypeUnit(SectionForceType forceType)
        {
            bool isMoment = (forceType == SectionForceType.Mx ||
                            forceType == SectionForceType.My ||
                            forceType == SectionForceType.Mz);

            if (isMoment)
            {
                return $"{SelectedForceUnit}·{SelectedLengthUnit}";
            }
            else
            {
                return SelectedForceUnit.ToString();
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
                {
                    if (f.Mesh != null && f.Mesh.IsValid)
                        bbox.Union(f.Mesh.GetBoundingBox(false));
                }
                
                foreach (var d in _preview.Labels)
                    bbox.Union(new BoundingBox(d.Location, d.Location));
                
                return bbox;
            }
        }

        protected override Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("38F2E784-9423-41C5-BDB7-1F035C7E4241");

        /// <summary>
        /// データの書き込み（保存時）
        /// </summary>
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("ShowFilled", ShowFilled);
            writer.SetBoolean("ShowNumbers", ShowNumbers);
            writer.SetBoolean("ShowLegend", ShowLegend);
            writer.SetInt32("SelectedForceType", (int)SelectedForceType);
            writer.SetBoolean("IsSectionForcesTabExpanded", IsSectionForcesTabExpanded);
            writer.SetInt32("SelectedForceUnit", (int)SelectedForceUnit);
            writer.SetInt32("SelectedLengthUnit", (int)SelectedLengthUnit);
            return base.Write(writer);
        }

        /// <summary>
        /// データの読み込み（読込時）
        /// </summary>
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ShowFilled"))
                ShowFilled = reader.GetBoolean("ShowFilled");
            if (reader.ItemExists("ShowNumbers"))
                ShowNumbers = reader.GetBoolean("ShowNumbers");
            if (reader.ItemExists("ShowLegend"))
                ShowLegend = reader.GetBoolean("ShowLegend");
            if (reader.ItemExists("SelectedForceType"))
                SelectedForceType = (SectionForceType)reader.GetInt32("SelectedForceType");
            if (reader.ItemExists("IsSectionForcesTabExpanded"))
                IsSectionForcesTabExpanded = reader.GetBoolean("IsSectionForcesTabExpanded");
            if (reader.ItemExists("SelectedForceUnit"))
                SelectedForceUnit = (ForceUnit)reader.GetInt32("SelectedForceUnit");
            if (reader.ItemExists("SelectedLengthUnit"))
                SelectedLengthUnit = (LengthUnit)reader.GetInt32("SelectedLengthUnit");
            return base.Read(reader);
        }

        /// <summary>
        /// 力の単位変換係数を取得
        /// </summary>
        private double GetForceConversionFactor()
        {
            switch (SelectedForceUnit)
            {
                case ForceUnit.N:
                    return 1.0; // N (基準単位)
                case ForceUnit.kN:
                    return 0.001; // N → kN
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// 長さの単位変換係数を取得
        /// </summary>
        private double GetLengthConversionFactor()
        {
            switch (SelectedLengthUnit)
            {
                case LengthUnit.mm:
                    return 1.0; // mm (基準単位)
                case LengthUnit.m:
                    return 0.001; // mm → m
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// モーメントの単位変換係数を取得（力 × 長さ）
        /// </summary>
        private double GetMomentConversionFactor()
        {
            return GetForceConversionFactor() * GetLengthConversionFactor();
        }

        /// <summary>
        /// 断面力値を現在の単位系に変換
        /// </summary>
        /// <param name="value">内部単位系の値（N または N·mm）</param>
        /// <param name="isMoment">モーメントかどうか</param>
        /// <returns>変換後の値</returns>
        public double ConvertSectionForceValue(double value, bool isMoment)
        {
            if (isMoment)
            {
                return value * GetMomentConversionFactor();
            }
            else
            {
                return value * GetForceConversionFactor();
            }
        }
    }
}