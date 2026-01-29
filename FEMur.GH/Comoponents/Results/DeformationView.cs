using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Models;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Results;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// DeformationView コンポーネント - 変形後の形状を可視化
    /// </summary>
    public class DeformationView : GH_Component
    {
        #region Properties

        // 表示設定（UIから切り替え）
        public bool ShowNumbers { get; set; } = false;
        public bool ShowLegend { get; set; } = true;

        // 変形方向の選択
        public DeformationDirection SelectedDirection { get; set; } = DeformationDirection.Dxyz;

        // 展開タブの状態
        public bool IsDisplayTabExpanded { get; set; } = false;

        // 入力キャッシュ
        private Model _model;
        private double _scale = 1.0;
        private double _autoScale = 1.0; // 自動計算されたスケール

        // プレビューキャッシュ
        private List<Line> _deformedLines = new List<Line>();
        private List<Line> _originalLines = new List<Line>();
        private Dictionary<int, DeformationInfo> _nodeDeformations = new Dictionary<int, DeformationInfo>();

        // 統計情報キャッシュ
        private DeformationStatistics _statistics = new DeformationStatistics();

        // 自由度の定数
        private const int DOF_PER_NODE = 6;

        #endregion

        #region Enums

        public enum DeformationDirection
        {
            Dx,   // X方向のみ
            Dy,   // Y方向のみ
            Dz,   // Z方向のみ
            Dxy,  // XY平面内の変形
            Dyz,  // YZ平面内の変形
            Dzx,  // ZX平面内の変形
            Dxyz  // 3次元合成変形
        }

        #endregion

        #region Constructor

        public DeformationView()
          : base("DeformationView(FEMur)", "DeformationView",
              "Visualize deformed shape from analyzed model with auto-scaling",
              "FEMur", "9.Result")
        {
        }

        protected override Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("4A8B3C9D-1E2F-4A5B-8C7D-9E0F1A2B3C4D");

        #endregion

        #region Input/Output Parameters

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model with computed results", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "Display scale factor (multiplied with auto-scale)", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model (pass-through)", GH_ParamAccess.item);
            pManager.AddLineParameter("DeformedModel", "DM", "Deformed model geometry (bake-able)", GH_ParamAccess.list);
        }

        #endregion

        #region Solve Instance

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _model = null;
            _scale = 1.0;
            _autoScale = 1.0;

            if (!DA.GetData(0, ref _model) || _model == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "AnalyzedModel is required");
                ClearCaches();
                return;
            }

            if (!DA.GetData(1, ref _scale))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Scale not set, using default 1.0");
                _scale = 1.0;
            }

            if (!_model.IsSolved || _model.Result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Model has not been solved yet.");
                ClearCaches();
                DA.SetData(0, _model); 
                return;
            }

            // モデルサイズに基づく自動スケール計算
            _autoScale = ComputeAutoScale(_model);

            // 最終スケール = 自動スケール × 入力スケール
            double finalScale = _autoScale * _scale;

            // 変形形状を生成
            GenerateDeformedGeometry(_model, finalScale);

            // 統計情報を計算
            CalculateStatistics();

            // 出力
            DA.SetData(0, _model);           // 1個目の出力: AnalyzedModel（パススルー）
            DA.SetDataList(1, _deformedLines); // 2個目の出力: DeformedModel
        }

        #endregion

        #region Geometry Generation

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        private void ClearCaches()
        {
            _deformedLines.Clear();
            _originalLines.Clear();
            _nodeDeformations.Clear();
            _statistics = new DeformationStatistics();
        }

        /// <summary>
        /// 変形後の形状を生成
        /// </summary>
        private void GenerateDeformedGeometry(Model model, double scale)
        {
            ClearCaches();

            if (model.Result == null || model.Result.NodalDisplacements == null)
                return;

            var displacements = model.Result.NodalDisplacements;

            if (displacements.Count == 0)
                return;

            // 節点変位情報を計算
            CalculateNodeDeformations(model, scale);

            // 要素の変形後の線を生成
            GenerateDeformedLines(model);

            // 元の形状も保存（比較用）
            GenerateOriginalLines(model);
        }

        /// <summary>
        /// 節点変位情報を計算
        /// </summary>
        private void CalculateNodeDeformations(Model model, double scale)
        {
            _nodeDeformations.Clear();

            var displacements = model.Result.NodalDisplacements;

            for (int i = 0; i < model.Nodes.Count; i++)
            {
                var node = model.Nodes[i];
                
                // 変位ベクトルから各成分を取得（6自由度: Ux, Uy, Uz, Rx, Ry, Rz）
                int baseIndex = i * DOF_PER_NODE;
                
                if (baseIndex + DOF_PER_NODE > displacements.Count)
                    continue;

                double ux = displacements[baseIndex + 0];
                double uy = displacements[baseIndex + 1];
                double uz = displacements[baseIndex + 2];
                double rx = displacements[baseIndex + 3];
                double ry = displacements[baseIndex + 4];
                double rz = displacements[baseIndex + 5];

                var originalPos = new Point3d(node.Position.X, node.Position.Y, node.Position.Z);
                
                // 選択された方向に応じて変形を適用
                Point3d deformedPos = CalculateDeformedPosition(originalPos, ux, uy, uz, scale);

                // 各方向の変形量を計算
                double dx = Math.Abs(ux);
                double dy = Math.Abs(uy);
                double dz = Math.Abs(uz);
                double dxy = Math.Sqrt(ux * ux + uy * uy);
                double dyz = Math.Sqrt(uy * uy + uz * uz);
                double dzx = Math.Sqrt(uz * uz + ux * ux);
                double dxyz = Math.Sqrt(ux * ux + uy * uy + uz * uz);

                _nodeDeformations[node.Id] = new DeformationInfo
                {
                    OriginalPosition = originalPos,
                    DeformedPosition = deformedPos,
                    DisplacementVector = new Vector3d(ux, uy, uz),
                    Ux = ux,
                    Uy = uy,
                    Uz = uz,
                    Rx = rx,
                    Ry = ry,
                    Rz = rz,
                    Dx = dx,
                    Dy = dy,
                    Dz = dz,
                    Dxy = dxy,
                    Dyz = dyz,
                    Dzx = dzx,
                    Dxyz = dxyz
                };
            }
        }

        /// <summary>
        /// 選択された方向に応じて変形後の位置を計算
        /// </summary>
        private Point3d CalculateDeformedPosition(Point3d original, double ux, double uy, double uz, double scale)
        {
            switch (SelectedDirection)
            {
                case DeformationDirection.Dx:
                    return new Point3d(original.X + ux * scale, original.Y, original.Z);
                case DeformationDirection.Dy:
                    return new Point3d(original.X, original.Y + uy * scale, original.Z);
                case DeformationDirection.Dz:
                    return new Point3d(original.X, original.Y, original.Z + uz * scale);
                case DeformationDirection.Dxy:
                    return new Point3d(original.X + ux * scale, original.Y + uy * scale, original.Z);
                case DeformationDirection.Dyz:
                    return new Point3d(original.X, original.Y + uy * scale, original.Z + uz * scale);
                case DeformationDirection.Dzx:
                    return new Point3d(original.X + ux * scale, original.Y, original.Z + uz * scale);
                case DeformationDirection.Dxyz:
                default:
                    return new Point3d(original.X + ux * scale, original.Y + uy * scale, original.Z + uz * scale);
            }
        }

        /// <summary>
        /// 統計情報を計算
        /// </summary>
        private void CalculateStatistics()
        {
            _statistics = new DeformationStatistics();

            if (_nodeDeformations.Count == 0)
                return;

            double maxDx = 0, maxDy = 0, maxDz = 0, maxDxyz = 0;
            int nodeDx = -1, nodeDy = -1, nodeDz = -1, nodeDxyz = -1;

            foreach (var kvp in _nodeDeformations)
            {
                var info = kvp.Value;

                if (info.Dx > maxDx)
                {
                    maxDx = info.Dx;
                    nodeDx = kvp.Key;
                }

                if (info.Dy > maxDy)
                {
                    maxDy = info.Dy;
                    nodeDy = kvp.Key;
                }

                if (info.Dz > maxDz)
                {
                    maxDz = info.Dz;
                    nodeDz = kvp.Key;
                }

                if (info.Dxyz > maxDxyz)
                {
                    maxDxyz = info.Dxyz;
                    nodeDxyz = kvp.Key;
                }
            }

            _statistics.MaxDx = maxDx;
            _statistics.MaxDy = maxDy;
            _statistics.MaxDz = maxDz;
            _statistics.MaxDxyz = maxDxyz;
            _statistics.NodeIdMaxDx = nodeDx;
            _statistics.NodeIdMaxDy = nodeDy;
            _statistics.NodeIdMaxDz = nodeDz;
            _statistics.NodeIdMaxDxyz = nodeDxyz;
        }

        /// <summary>
        /// 変形後の要素線を生成
        /// </summary>
        private void GenerateDeformedLines(Model model)
        {
            _deformedLines.Clear();

            foreach (var element in model.Elements)
            {
                if (element.NodeIds == null || element.NodeIds.Count < 2)
                    continue;

                int nodeId_i = element.NodeIds[0];
                int nodeId_j = element.NodeIds[1];

                if (!_nodeDeformations.ContainsKey(nodeId_i) || !_nodeDeformations.ContainsKey(nodeId_j))
                    continue;

                var pt_i = _nodeDeformations[nodeId_i].DeformedPosition;
                var pt_j = _nodeDeformations[nodeId_j].DeformedPosition;

                _deformedLines.Add(new Line(pt_i, pt_j));
            }
        }

        /// <summary>
        /// 元の要素線を生成
        /// </summary>
        private void GenerateOriginalLines(Model model)
        {
            _originalLines.Clear();

            foreach (var element in model.Elements)
            {
                if (element.NodeIds == null || element.NodeIds.Count < 2)
                    continue;

                int nodeId_i = element.NodeIds[0];
                int nodeId_j = element.NodeIds[1];

                if (!_nodeDeformations.ContainsKey(nodeId_i) || !_nodeDeformations.ContainsKey(nodeId_j))
                    continue;

                var pt_i = _nodeDeformations[nodeId_i].OriginalPosition;
                var pt_j = _nodeDeformations[nodeId_j].OriginalPosition;

                _originalLines.Add(new Line(pt_i, pt_j));
            }
        }

        #endregion

        #region Auto Scale Calculation

        /// <summary>
        /// モデルの大きさと変位量に基づいて自動スケールを計算
        /// </summary>
        private double ComputeAutoScale(Model model)
        {
            if (model == null || model.Nodes == null || model.Nodes.Count == 0)
                return 1.0;

            if (model.Result == null || model.Result.NodalDisplacements == null || model.Result.NodalDisplacements.Count == 0)
                return 1.0;

            // 1. モデルの特性寸法を計算（バウンディングボックスの対角線長さ）
            double modelSize = CalculateModelCharacteristicLength(model);

            if (modelSize <= 0)
                return 1.0;

            // 2. 最大変位量を取得
            double maxDisplacement = GetMaxDisplacement(model);

            if (maxDisplacement <= 0)
                return 1.0;

            // 3. 自動スケール = (モデルサイズ × 0.1) / 最大変位量
            // 変形がモデルサイズの約10%になるように調整
            double autoScale = (modelSize * 0.1) / maxDisplacement;

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
        /// 最大変位量を取得
        /// </summary>
        private double GetMaxDisplacement(Model model)
        {
            if (model.Result == null || model.Result.NodalDisplacements == null)
                return 0.0;

            var displacements = model.Result.NodalDisplacements;
            double maxDisplacement = 0.0;

            for (int i = 0; i < model.Nodes.Count; i++)
            {
                int baseIndex = i * DOF_PER_NODE;
                
                if (baseIndex + DOF_PER_NODE > displacements.Count)
                    continue;

                double ux = displacements[baseIndex + 0];
                double uy = displacements[baseIndex + 1];
                double uz = displacements[baseIndex + 2];

                double magnitude = Math.Sqrt(ux * ux + uy * uy + uz * uz);
                maxDisplacement = Math.Max(maxDisplacement, magnitude);
            }

            return maxDisplacement;
        }

        #endregion

        #region Viewport Drawing

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_model == null || _model.Result == null)
                return;

            var display = args.Display;

            // 元の形状を薄いグレーで描画
            foreach (var line in _originalLines)
            {
                display.DrawLine(line, Color.FromArgb(100, 150, 150, 150), 1);
            }

            // 変形後の形状を青で描画
            foreach (var line in _deformedLines)
            {
                display.DrawLine(line, Color.Blue, 2);
            }

            // 数値表示が有効な場合、節点変位を描画
            if (ShowNumbers)
            {
                DrawDeformationLabels(display);
            }

            // 凡例を描画
            if (ShowLegend)
            {
                DrawLegend(args);
            }
        }

        /// <summary>
        /// 変位数値ラベルを描画
        /// </summary>
        private void DrawDeformationLabels(Rhino.Display.DisplayPipeline display)
        {
            foreach (var kvp in _nodeDeformations)
            {
                var info = kvp.Value;
                
                // 選択された方向に応じて表示する値を決定（ラベルなし、数値のみ）
                double value = GetDeformationValue(info);
                string text = $"{value:F3}";
                
                display.Draw2dText(text, Color.Black, info.DeformedPosition, true, 12);
            }
        }

        /// <summary>
        /// 選択された方向の変形量を取得
        /// </summary>
        private double GetDeformationValue(DeformationInfo info)
        {
            switch (SelectedDirection)
            {
                case DeformationDirection.Dx: return info.Dx;
                case DeformationDirection.Dy: return info.Dy;
                case DeformationDirection.Dz: return info.Dz;
                case DeformationDirection.Dxy: return info.Dxy;
                case DeformationDirection.Dyz: return info.Dyz;
                case DeformationDirection.Dzx: return info.Dzx;
                case DeformationDirection.Dxyz: return info.Dxyz;
                default: return info.Dxyz;
            }
        }

        /// <summary>
        /// 方向のラベルを取得
        /// </summary>
        private string GetDirectionLabel()
        {
            switch (SelectedDirection)
            {
                case DeformationDirection.Dx: return "Dx";
                case DeformationDirection.Dy: return "Dy";
                case DeformationDirection.Dz: return "Dz";
                case DeformationDirection.Dxy: return "Dxy";
                case DeformationDirection.Dyz: return "Dyz";
                case DeformationDirection.Dzx: return "Dzx";
                case DeformationDirection.Dxyz: return "Dxyz";
                default: return "D";
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

            // 凡例の設定（応力Legendに近づける）
            int margin = 40;
            int topMargin = 20;
            int lineHeight = 20;

            // 右側に配置（応力Legendの位置に近づける：右端からの距離を短く）
            int legendX = viewportWidth - 220 - margin;
            int legendY = topMargin;

            // タイトル
            display.Draw2dText("Deformation Summary", Color.Black, new Point2d(legendX, legendY), false, 14, "Arial");
            legendY += lineHeight + 5;

            // 統計情報を描画
            display.Draw2dText($"x-dir = {_statistics.MaxDx:F6}  Node = {_statistics.NodeIdMaxDx}", 
                Color.Black, new Point2d(legendX, legendY), false, 11, "Arial");
            legendY += lineHeight;

            display.Draw2dText($"y-dir = {_statistics.MaxDy:F6}  Node = {_statistics.NodeIdMaxDy}", 
                Color.Black, new Point2d(legendX, legendY), false, 11, "Arial");
            legendY += lineHeight;

            display.Draw2dText($"z-dir = {_statistics.MaxDz:F6}  Node = {_statistics.NodeIdMaxDz}", 
                Color.Black, new Point2d(legendX, legendY), false, 11, "Arial");
            legendY += lineHeight;

            display.Draw2dText($"comb.-dir = {_statistics.MaxDxyz:F6}  Node = {_statistics.NodeIdMaxDxyz}", 
                Color.Black, new Point2d(legendX, legendY), false, 11, "Arial");
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                BoundingBox bbox = BoundingBox.Empty;

                foreach (var line in _originalLines)
                    bbox.Union(line.BoundingBox);

                foreach (var line in _deformedLines)
                    bbox.Union(line.BoundingBox);

                return bbox;
            }
        }

        #endregion

        #region Custom Attributes

        public override void CreateAttributes()
        {
            m_attributes = new DeformationViewAttributes(this);
        }

        #endregion

        #region Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("ShowNumbers", ShowNumbers);
            writer.SetBoolean("ShowLegend", ShowLegend);
            writer.SetInt32("SelectedDirection", (int)SelectedDirection);
            writer.SetBoolean("IsDisplayTabExpanded", IsDisplayTabExpanded);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ShowNumbers"))
                ShowNumbers = reader.GetBoolean("ShowNumbers");
            if (reader.ItemExists("ShowLegend"))
                ShowLegend = reader.GetBoolean("ShowLegend");
            if (reader.ItemExists("SelectedDirection"))
                SelectedDirection = (DeformationDirection)reader.GetInt32("SelectedDirection");
            if (reader.ItemExists("IsDisplayTabExpanded"))
                IsDisplayTabExpanded = reader.GetBoolean("IsDisplayTabExpanded");
            return base.Read(reader);
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 節点変位情報
        /// </summary>
        private class DeformationInfo
        {
            public Point3d OriginalPosition { get; set; }
            public Point3d DeformedPosition { get; set; }
            public Vector3d DisplacementVector { get; set; }
            public double Ux { get; set; }
            public double Uy { get; set; }
            public double Uz { get; set; }
            public double Rx { get; set; }
            public double Ry { get; set; }
            public double Rz { get; set; }
            public double Dx { get; set; }
            public double Dy { get; set; }
            public double Dz { get; set; }
            public double Dxy { get; set; }
            public double Dyz { get; set; }
            public double Dzx { get; set; }
            public double Dxyz { get; set; }
        }

        /// <summary>
        /// 変形統計情報
        /// </summary>
        private class DeformationStatistics
        {
            public double MaxDx { get; set; }
            public double MaxDy { get; set; }
            public double MaxDz { get; set; }
            public double MaxDxyz { get; set; }
            public int NodeIdMaxDx { get; set; } = -1;
            public int NodeIdMaxDy { get; set; } = -1;
            public int NodeIdMaxDz { get; set; } = -1;
            public int NodeIdMaxDxyz { get; set; } = -1;
        }

        #endregion
    }
}