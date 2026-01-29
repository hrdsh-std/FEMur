using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Models;
using FEMur.Results;
using FEMur.Common.Units;
using FEMurGH.Extensions;
using FEMurGH.Common.Drawing;
using FEMurGH.Common.Utils;

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
        public bool ShowNumbers { get; set; } = true;

        // 単位選択（FEMur.Common.Unitsの列挙型を使用）
        public ForceUnit SelectedForceUnit { get; set; } = ForceUnit.N;
        public LengthUnit SelectedLengthUnit { get; set; } = LengthUnit.mm;

        // 展開タブの状態
        public bool IsReactionForceTabExpanded { get; set; } = false;

        // 描画用キャッシュのみ（SolveInstance実行後に保持）
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
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model with computed results (from LinearStaticSolver)", GH_ParamAccess.item);
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
            // ローカル変数として扱う（フィールドに保存しない）
            Model model = null;
            double scale = 1.0;

            if (!DA.GetData(0, ref model) || model == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "AnalyzedModel is required");
                ClearCaches();
                return;
            }

            if (!DA.GetData(1, ref scale))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Scale not set, using default 1.0");
                scale = 1.0;
            }

            // 未解析モデルのチェック（毎回入力から確認）
            if (!model.IsSolved || model.Result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
                    "Model has not been solved yet.");
                ClearCaches();
                DA.SetData(0, model);
                return;
            }

            if (model.Result.ReactionForces == null || model.Result.ReactionForces.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No reaction forces found in the result.");
                ClearCaches();
                DA.SetData(0, model);
                return;
            }

            // 力とモーメントで別々に自動スケール計算
            double forceAutoScale = ComputeAutoScale(model);
            double momentAutoScale = ComputeMomentAutoScale(model);

            // 最終スケール = 自動スケール × 入力スケール
            double finalForceScale = forceAutoScale * scale;
            double finalMomentScale = momentAutoScale * scale;

            // 反力矢印を生成（力とモーメントで別々のスケール）
            GenerateReactionArrows(model, finalForceScale, finalMomentScale);

            // 出力（入力をそのまま渡す）
            DA.SetData(0, model);
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

            Point3d startPoint = origin - direction * Math.Abs(length);

            return new ArrowGeometry
            {
                Start = startPoint,
                End = origin,
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

            // ModelCalculatorを使用して自動スケールを計算
            return ModelCalculator.CalculateForceAutoScale(model, maxForce, 0.15);
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

            // ModelCalculatorを使用して自動スケールを計算
            return ModelCalculator.CalculateMomentAutoScale(model, maxMoment, 0.1);
        }

        #endregion

        #region Viewport Drawing

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            // キャッシュされた矢印ジオメトリを描画
            // （modelの状態は関係なく、SolveInstanceで生成されたジオメトリのみ描画）
            if (_forceArrows.Count == 0 && _momentArrows.Count == 0)
                return;

            var display = args.Display;

            // 力の矢印を描画
            foreach (var arrow in _forceArrows)
            {
                double convertedValue = UnitConverter.ConvertForce(arrow.Magnitude, SelectedForceUnit);
                string valueText = ShowNumbers ? $"{convertedValue:F1}" : null;
                ArrowRenderer.DrawForceArrow(display, arrow, ShowNumbers, valueText, 12);
            }

            // モーメントの矢印を描画
            foreach (var moment in _momentArrows)
            {
                double convertedValue = UnitConverter.ConvertMoment(moment.Magnitude, SelectedForceUnit, SelectedLengthUnit);
                string valueText = ShowNumbers ? $"{convertedValue:F1}" : null;
                ArrowRenderer.DrawMomentArrow(display, moment, ShowNumbers, valueText, 12);
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
            writer.SetBoolean("IsReactionsTabExpanded", IsReactionForceTabExpanded);
            writer.SetInt32("SelectedForceUnit", (int)SelectedForceUnit);
            writer.SetInt32("SelectedLengthUnit", (int)SelectedLengthUnit);
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
                IsReactionForceTabExpanded = reader.GetBoolean("IsReactionsTabExpanded");
            if (reader.ItemExists("SelectedForceUnit"))
                SelectedForceUnit = (ForceUnit)reader.GetInt32("SelectedForceUnit");
            if (reader.ItemExists("SelectedLengthUnit"))
                SelectedLengthUnit = (LengthUnit)reader.GetInt32("SelectedLengthUnit");
            return base.Read(reader);
        }

        #endregion
    }
}