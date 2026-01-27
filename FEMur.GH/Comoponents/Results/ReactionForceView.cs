using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Models;
using FEMur.Results;
using FEMurGH.Extensions;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// ReactionForceView コンポーネント - 反力を矢印で可視化
    /// </summary>
    public class ReactionForceView : GH_Component
    {
        #region Properties

        // 表示設定（UIから切り替え - 複数選択可能）
        public bool ShowFx { get; set; } = false;
        public bool ShowFy { get; set; } = false;
        public bool ShowFz { get; set; } = false;
        public bool ShowMx { get; set; } = false;
        public bool ShowMy { get; set; } = false;
        public bool ShowMz { get; set; } = false;

        // 展開タブの状態
        public bool IsReactionsTabExpanded { get; set; } = false;

        // 入力キャッシュ
        private Model _model;
        private double _scale = 1.0;
        private double _autoScale = 1.0;

        // 描画キャッシュ
        private List<ArrowGeometry> _forceArrows = new List<ArrowGeometry>();
        private List<MomentArrowGeometry> _momentArrows = new List<MomentArrowGeometry>();

        #endregion

        #region Constructor

        public ReactionForceView()
          : base("ReactionForceView(FEMur)", "ReactionForceView",
              "Visualize reaction forces and moments as arrows at support points",
              "FEMur", "9.Result")
        {
        }

        protected override Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("5B9C4D8E-2F3A-4B6C-9D8E-0F1A2B3C4D5E");

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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Model has not been solved yet. Run LinearStaticSolver first.");
                ClearCaches();
                DA.SetData(0, _model);
                return;
            }

            if (_model.Result.ReactionForces == null || _model.Result.ReactionForces.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No reaction forces found in the result.");
                ClearCaches();
                DA.SetData(0, _model);
                return;
            }

            // 力とモーメントで別々に自動スケール計算
            double forceAutoScale = ComputeAutoScale(_model);
            double momentAutoScale = ComputeMomentAutoScale(_model);

            // 最終スケール = 自動スケール × 入力スケール
            double finalForceScale = forceAutoScale * _scale;
            double finalMomentScale = momentAutoScale * _scale;

            // 反力矢印を生成（力とモーメントで別々のスケール）
            GenerateReactionArrows(_model, finalForceScale, finalMomentScale);

            // 出力（入力をそのまま渡す）
            DA.SetData(0, _model);
        }

        #endregion

        #region Geometry Generation

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        private void ClearCaches()
        {
            _forceArrows.Clear();
            _momentArrows.Clear();
        }

        /// <summary>
        /// 反力矢印を生成
        /// </summary>
        private void GenerateReactionArrows(Model model, double forceScale, double momentScale)
        {
            ClearCaches();

            foreach (var reaction in model.Result.ReactionForces)
            {
                // 節点位置を取得
                var node = model.Nodes.FirstOrDefault(n => n.Id == reaction.NodeId);
                if (node == null) continue;

                Point3d position = node.ToRhinoPoint();

                // 力の矢印（Fx, Fy, Fz）- forceScaleを使用
                if (ShowFx && Math.Abs(reaction.Fx) > 1e-10)
                {
                    _forceArrows.Add(CreateForceArrow(position, Vector3d.XAxis, 
                        reaction.Fx, forceScale, Color.Red, "Fx"));
                }

                if (ShowFy && Math.Abs(reaction.Fy) > 1e-10)
                {

                    _forceArrows.Add(CreateForceArrow(position, Vector3d.YAxis, 
                        reaction.Fy, forceScale, Color.Green, "Fy"));
                }

                if (ShowFz && Math.Abs(reaction.Fz) > 1e-10)
                {
                    _forceArrows.Add(CreateForceArrow(position, Vector3d.ZAxis, 
                        reaction.Fz, forceScale, Color.Blue, "Fz"));
                }

                // モーメントの矢印（Mx, My, Mz）- momentScaleを使用
                if (ShowMx && Math.Abs(reaction.Mx) > 1e-10)
                {
                    _momentArrows.Add(CreateMomentArrow(position, Vector3d.XAxis, 
                        reaction.Mx, momentScale, Color.DarkRed, "Mx"));
                }

                if (ShowMy && Math.Abs(reaction.My) > 1e-10)
                {
                    _momentArrows.Add(CreateMomentArrow(position, Vector3d.YAxis, 
                        reaction.My, momentScale, Color.DarkGreen, "My"));
                }

                if (ShowMz && Math.Abs(reaction.Mz) > 1e-10)
                {
                    _momentArrows.Add(CreateMomentArrow(position, Vector3d.ZAxis, 
                        reaction.Mz, momentScale, Color.DarkBlue, "Mz"));
                }
            }
        }

        /// <summary>
        /// 力の矢印を作成
        /// </summary>
        private ArrowGeometry CreateForceArrow(Point3d origin, Vector3d axis, double magnitude, 
            double scale, Color color, string label)
        {
            // 矢印の長さを決定（スケール適用）
            double length = magnitude * scale;
            
            // 矢印の向き（符号を反映）
            Vector3d direction = axis * (magnitude > 0 ? 1 : -1);
            direction.Unitize();

            Point3d endPoint = origin + direction * Math.Abs(length);

            return new ArrowGeometry
            {
                Start = origin,
                End = endPoint,
                Direction = direction,
                Magnitude = magnitude,
                Color = color,
                Label = label
            };
        }

        /// <summary>
        /// モーメントの矢印を作成（右ネジ系、二重矢印）
        /// </summary>
        private MomentArrowGeometry CreateMomentArrow(Point3d origin, Vector3d axis, double magnitude, 
            double scale, Color color, string label)
        {
            // モーメントの大きさに応じて半径を決定
            // スケールを適用
            double radius = Math.Abs(magnitude) * scale;

            // 最小半径と最大半径を設定
            double minRadius = 20.0;  // 最小20mm（見やすく）
            double maxRadius = 500.0; // 最大500mm
            radius = Math.Max(minRadius, Math.Min(maxRadius, radius));

            // 右ネジの法則: 軸の正方向を向いて見た時
            // 正のモーメント → 反時計回り（CCW）
            // 負のモーメント → 時計回り（CW）
            // 描画上は逆になるため、符号を反転
            bool clockwise = magnitude > 0;

            return new MomentArrowGeometry
            {
                Center = origin,
                Axis = axis,
                Radius = radius,
                Magnitude = magnitude,
                Clockwise = clockwise,
                Color = color,
                Label = label
            };
        }

        #endregion

        #region Auto Scale Calculation

        /// <summary>
        /// 反力の大きさに基づいて自動スケールを計算
        /// </summary>
        private double ComputeAutoScale(Model model)
        {
            if (model == null || model.Nodes == null || model.Nodes.Count == 0)
                return 1.0;

            if (model.Result == null || model.Result.ReactionForces == null || model.Result.ReactionForces.Count == 0)
                return 1.0;

            // モデルサイズを取得
            double modelSize = CalculateModelCharacteristicLength(model);
            if (modelSize <= 0) return 1.0;

            // 最大反力（力）を取得
            double maxForce = 0.0;

            foreach (var reaction in model.Result.ReactionForces)
            {
                maxForce = Math.Max(maxForce, Math.Abs(reaction.Fx));
                maxForce = Math.Max(maxForce, Math.Abs(reaction.Fy));
                maxForce = Math.Max(maxForce, Math.Abs(reaction.Fz));
            }

            if (maxForce <= 0)
                return 1.0;

            // 力の自動スケール = モデルサイズの15% / 最大反力
            // より大きな矢印にするため係数を増やす
            double forceScale = (modelSize * 0.15) / maxForce;

            return forceScale;
        }

        /// <summary>
        /// モーメント用の自動スケールを計算
        /// </summary>
        private double ComputeMomentAutoScale(Model model)
        {
            if (model == null || model.Nodes == null || model.Nodes.Count == 0)
                return 1.0;

            if (model.Result == null || model.Result.ReactionForces == null || model.Result.ReactionForces.Count == 0)
                return 1.0;

            // モデルサイズを取得
            double modelSize = CalculateModelCharacteristicLength(model);
            if (modelSize <= 0) return 1.0;

            // 最大モーメントを取得
            double maxMoment = 0.0;

            foreach (var reaction in model.Result.ReactionForces)
            {
                maxMoment = Math.Max(maxMoment, Math.Abs(reaction.Mx));
                maxMoment = Math.Max(maxMoment, Math.Abs(reaction.My));
                maxMoment = Math.Max(maxMoment, Math.Abs(reaction.Mz));
            }

            if (maxMoment <= 0)
                return 1.0;

            // モーメントの自動スケール = モデルサイズの10% / 最大モーメント
            double momentScale = (modelSize * 0.1) / maxMoment;

            return momentScale;
        }

        /// <summary>
        /// モデルの特性寸法を計算
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

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        #endregion

        #region Viewport Drawing

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_model == null || _model.Result == null)
                return;

            var display = args.Display;

            // 力の矢印を描画
            foreach (var arrow in _forceArrows)
            {
                DrawForceArrow(display, arrow);
            }

            // モーメントの矢印を描画
            foreach (var moment in _momentArrows)
            {
                DrawMomentArrow(display, moment);
            }
        }

        /// <summary>
        /// 力の矢印を描画
        /// </summary>
        private void DrawForceArrow(Rhino.Display.DisplayPipeline display, ArrowGeometry arrow)
        {
            // 矢印の線
            display.DrawArrow(new Line(arrow.Start, arrow.End), arrow.Color, 20, 5);

            // ラベル（数値のみ）
            string labelText = $"{arrow.Magnitude:F1}";
            display.Draw2dText(labelText, arrow.Color, arrow.End, true, 12);
        }

        /// <summary>
        /// モーメントの矢印を描画（二重矢印、右ネジ系）
        /// </summary>
        private void DrawMomentArrow(Rhino.Display.DisplayPipeline display, MomentArrowGeometry moment)
        {
            // モーメントの大きさが小さすぎる場合はスキップ
            if (moment.Radius < 0.01)
                return;

            // 回転軸に垂直な平面上に円弧を描画
            Vector3d axis = moment.Axis;
            axis.Unitize();

            // 軸ごとに作業平面を定義（右手系を保証）
            Vector3d perpendicular1, perpendicular2;

            if (Math.Abs(axis.X - 1.0) < 0.01) // X軸周りのモーメント（Mx）
            {
                // X軸周り: Y軸を基準、Z軸方向へ
                perpendicular1 = Vector3d.YAxis;
                perpendicular2 = Vector3d.ZAxis;
            }
            else if (Math.Abs(axis.Y - 1.0) < 0.01) // Y軸周りのモーメント（My）
            {
                // Y軸周り: Z軸を基準、X軸方向へ
                perpendicular1 = Vector3d.ZAxis;
                perpendicular2 = Vector3d.XAxis;
            }
            else if (Math.Abs(axis.Z - 1.0) < 0.01) // Z軸周りのモーメント（Mz）
            {
                // Z軸周り: X軸を基準、Y軸方向へ
                perpendicular1 = Vector3d.XAxis;
                perpendicular2 = Vector3d.YAxis;
            }
            else // 一般的な軸（念のため）
            {
                // フォールバック: 従来のロジック
                if (Math.Abs(axis.X) < 0.9)
                {
                    perpendicular1 = Vector3d.CrossProduct(axis, Vector3d.XAxis);
                }
                else
                {
                    perpendicular1 = Vector3d.CrossProduct(axis, Vector3d.YAxis);
                }
                perpendicular1.Unitize();
                perpendicular2 = Vector3d.CrossProduct(axis, perpendicular1);
                perpendicular2.Unitize();
            }

            // 円弧を描画（右ネジの法則）
            int segments = 32;
            double arcAngle = 1.5 * Math.PI; // 270度の円弧
            double angleStep = arcAngle / segments;

            List<Point3d> arcPoints = new List<Point3d>();

            for (int i = 0; i <= segments; i++)
            {
                double angle = i * angleStep;

                // 右ネジの法則: 
                // 正のモーメント → 軸の正方向を向いて見た時、反時計回り
                // 負のモーメント → 軸の正方向を向いて見た時、時計回り
                // clockwise = true の時、右ネジ
                double actualAngle = moment.Clockwise ? angle : -angle;

                Vector3d radial = perpendicular1 * Math.Cos(actualAngle) + perpendicular2 * Math.Sin(actualAngle);
                Point3d point = moment.Center + radial * moment.Radius;
                arcPoints.Add(point);
            }

            // 外側の円弧を描画
            if (arcPoints.Count > 1)
            {
                for (int i = 0; i < arcPoints.Count - 1; i++)
                {
                    display.DrawLine(arcPoints[i], arcPoints[i + 1], moment.Color, 3);
                }

                // 矢印の先端（終点）
                Point3d arrowTip = arcPoints[arcPoints.Count - 1];
                
                // 接線ベクトルを計算
                if (arcPoints.Count >= 2)
                {
                    Vector3d tangent = arcPoints[arcPoints.Count - 1] - arcPoints[arcPoints.Count - 2];
                    tangent.Unitize();
                    
                    // 矢印ヘッドを描画（接線方向）
                    double arrowSize = moment.Radius * 0.3;
                    
                    // 法線方向（半径方向内向き）
                    Vector3d radialIn = moment.Center - arrowTip;
                    radialIn.Unitize();
                    
                    // 矢印の両側
                    Vector3d arrowLeft = tangent * (-arrowSize) + radialIn * (arrowSize * 0.3);
                    Vector3d arrowRight = tangent * (-arrowSize) - radialIn * (arrowSize * 0.3);

                    display.DrawLine(arrowTip, arrowTip + arrowLeft, moment.Color, 3);
                    display.DrawLine(arrowTip, arrowTip + arrowRight, moment.Color, 3);
                }

                // ラベル（数値のみ）
                string labelText = $"{moment.Magnitude:F1}";
                Point3d labelPos = arcPoints[arcPoints.Count / 2]; // 円弧の中央にラベル
                display.Draw2dText(labelText, moment.Color, labelPos, true, 12);
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                BoundingBox bbox = BoundingBox.Empty;

                foreach (var arrow in _forceArrows)
                {
                    bbox.Union(arrow.Start);
                    bbox.Union(arrow.End);
                }

                foreach (var moment in _momentArrows)
                {
                    bbox.Union(new BoundingBox(
                        new Point3d(moment.Center.X - moment.Radius, moment.Center.Y - moment.Radius, moment.Center.Z - moment.Radius),
                        new Point3d(moment.Center.X + moment.Radius, moment.Center.Y + moment.Radius, moment.Center.Z + moment.Radius)
                    ));
                }

                return bbox;
            }
        }

        #endregion

        #region Custom Attributes

        public override void CreateAttributes()
        {
            m_attributes = new ReactionForceViewAttributes(this);
        }

        #endregion

        #region Serialization

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("ShowFx", ShowFx);
            writer.SetBoolean("ShowFy", ShowFy);
            writer.SetBoolean("ShowFz", ShowFz);
            writer.SetBoolean("ShowMx", ShowMx);
            writer.SetBoolean("ShowMy", ShowMy);
            writer.SetBoolean("ShowMz", ShowMz);
            writer.SetBoolean("IsReactionsTabExpanded", IsReactionsTabExpanded);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ShowFx"))
                ShowFx = reader.GetBoolean("ShowFx");
            if (reader.ItemExists("ShowFy"))
                ShowFy = reader.GetBoolean("ShowFy");
            if (reader.ItemExists("ShowFz"))
                ShowFz = reader.GetBoolean("ShowFz");
            if (reader.ItemExists("ShowMx"))
                ShowMx = reader.GetBoolean("ShowMx");
            if (reader.ItemExists("ShowMy"))
                ShowMy = reader.GetBoolean("ShowMy");
            if (reader.ItemExists("ShowMz"))
                ShowMz = reader.GetBoolean("ShowMz");
            if (reader.ItemExists("IsReactionsTabExpanded"))
                IsReactionsTabExpanded = reader.GetBoolean("IsReactionsTabExpanded");
            return base.Read(reader);
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 力の矢印ジオメトリ
        /// </summary>
        private class ArrowGeometry
        {
            public Point3d Start { get; set; }
            public Point3d End { get; set; }
            public Vector3d Direction { get; set; }
            public double Magnitude { get; set; }
            public Color Color { get; set; }
            public string Label { get; set; }
        }

        /// <summary>
        /// モーメントの矢印ジオメトリ
        /// </summary>
        private class MomentArrowGeometry
        {
            public Point3d Center { get; set; }
            public Vector3d Axis { get; set; }
            public double Radius { get; set; }
            public double Magnitude { get; set; }
            public bool Clockwise { get; set; }
            public Color Color { get; set; }
            public string Label { get; set; }
        }

        #endregion
    }
}