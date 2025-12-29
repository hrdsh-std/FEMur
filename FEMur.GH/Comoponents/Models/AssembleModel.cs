using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Supports;
using FEMur.Loads;
using FEMur.Models;
using FEMur.CrossSections;

namespace FEMurGH.Comoponents.Models
{
    public class AssembleModel : GH_Component
    {
        // 表示設定
        public bool ShowNodeId { get; set; } = false;
        public bool ShowElementId { get; set; } = false;
        public bool ShowLoad { get; set; } = false;
        public bool ShowSupport { get; set; } = false;
        public bool ShowLocalAxis { get; set; } = false;
        public bool ShowCrossSection { get; set; } = false;
        public double LocalAxisScale { get; set; } = 0.3;

        // 展開タブの状態
        public bool IsDisplayTabExpanded { get; set; } = false;

        // キャッシュ
        private Model _cachedModel = null;
        private Dictionary<int, Point3d> _nodePositions = new Dictionary<int, Point3d>();
        private Dictionary<int, Point3d> _elementCenters = new Dictionary<int, Point3d>();
        private List<LocalAxisArrow> _localAxisArrows = new List<LocalAxisArrow>();
        private List<Brep> _crossSectionBreps = new List<Brep>();
        private List<Line> _elementLines = new List<Line>();

        /// <summary>
        /// Initializes a new instance of the AssembleModel class.
        /// </summary>
        public AssembleModel()
          : base("AssembleModel(FEMur)", "AssembleModel",
              "Assemble FEMur Model from Elements, Supports and Loads (Nodes are auto-generated from Elements)",
              "FEMur", "7.Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (List<FEMur.Elements.ElementBase>) - Nodes will be auto-generated from element Points", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "FEMur Supports (List<FEMur.Supports.Support>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "L", "FEMur Loads (List<FEMur.Loads.Load>)", GH_ParamAccess.list);

            // Supports と Loads はオプショナル(空リストでも解析可能)
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "FEMur Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var elements = new List<ElementBase>();
            var supports = new List<Support>();
            var loads = new List<Load>();

            if (!DA.GetDataList(0, elements))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Elements are required");
                return;
            }

            // オプショナル入力
            DA.GetDataList(1, supports);
            DA.GetDataList(2, loads);

            try
            {
                // ★ 修正: ElementのNodeIdsとPointsをリセット（キャッシュ問題対策）
                // Grasshopperでは同じElementオブジェクトが再利用されるため、
                // 前回の実行で設定されたNodeIdsをクリアする必要がある
                foreach (var element in elements)
                {
                    // NodeIdsが設定されている場合、Pointsから再生成できるようにリセット
                    if (element.NodeIds != null && element.NodeIds.Count > 0 && 
                        element.Points != null && element.Points.Count > 0)
                    {
                        element.NodeIds.Clear();
                    }
                }

                // ★ 修正: SupportとLoadのNodeIdもリセット（キャッシュ問題対策）
                foreach (var support in supports)
                {
                    if (support.Position.HasValue)
                    {
                        support.NodeId = -1; // 未割り当てに戻す
                    }
                }

                foreach (var load in loads)
                {
                    if (load is PointAction pointAction && pointAction.Position.HasValue)
                    {
                        pointAction.NodeId = -1; // 未割り当てに戻す
                    }
                }

                // Model を作成（Nodesは空リスト、Elementから自動生成される）
                var model = new Model(new List<Node>(), elements, supports, loads);
                _cachedModel = model;

                // 表示用の位置情報をキャッシュ
                CachePositions(model.Nodes, elements);

                // 要素の線材モデルを生成（常に表示)
                GenerateElementLines(elements, model.Nodes);

                // 局所座標系の矢印を生成
                if (ShowLocalAxis)
                {
                    GenerateLocalAxisArrows(elements, model.Nodes);
                }
                else
                {
                    _localAxisArrows.Clear();
                }

                // 断面形状の3Dモデルを生成
                if (ShowCrossSection)
                {
                    GenerateCrossSectionBreps(elements, model.Nodes);
                }
                else
                {
                    _crossSectionBreps.Clear();
                }

                DA.SetData(0, model);

            }
            catch (InvalidOperationException ex)
            {
                // Model検証エラーをGrasshopperの警告/エラーとして表示
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                return;
            }
        }

        /// <summary>
        /// カスタム属性を作成
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new AssembleModelAttributes(this);
        }

        /// <summary>
        /// ノードと要素の位置情報をキャッシュ
        /// </summary>
        private void CachePositions(List<Node> nodes, List<ElementBase> elements)
        {
            _nodePositions.Clear();
            _elementCenters.Clear();

            foreach (var node in nodes)
            {
                _nodePositions[node.Id] = new Point3d(node.Position.X, node.Position.Y, node.Position.Z);
            }

            foreach (var elem in elements)
            {
                if (elem.NodeIds != null && elem.NodeIds.Count >= 2)
                {
                    var n0Id = elem.NodeIds[0];
                    var n1Id = elem.NodeIds[1];
                    if (_nodePositions.ContainsKey(n0Id) && _nodePositions.ContainsKey(n1Id))
                    {
                        var p0 = _nodePositions[n0Id];
                        var p1 = _nodePositions[n1Id];
                        _elementCenters[elem.Id] = (p0 + p1) * 0.5;
                    }
                }
            }
        }

        /// <summary>
        /// 要素の線材モデルを生成（常に表示）
        /// </summary>
        private void GenerateElementLines(List<ElementBase> elements, List<Node> nodes)
        {
            _elementLines.Clear();

            var nodeById = nodes.ToDictionary(n => n.Id, n => n);

            foreach (var elem in elements)
            {
                if (elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                // LineElement（BeamElement等）のみ対応
                if (!(elem is LineElement))
                    continue;

                // ノード取得
                if (!nodeById.TryGetValue(elem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(elem.NodeIds[1], out Node n1)) continue;

                var p0 = new Point3d(n0.Position.X, n0.Position.Y, n0.Position.Z);
                var p1 = new Point3d(n1.Position.X, n1.Position.Y, n1.Position.Z);

                _elementLines.Add(new Line(p0, p1));
            }
        }

        /// <summary>
        /// 局所座標系の矢印を生成
        /// </summary>
        private void GenerateLocalAxisArrows(List<ElementBase> elements, List<Node> nodes)
        {
            _localAxisArrows.Clear();

            var nodeById = nodes.ToDictionary(n => n.Id, n => n);

            foreach (var elem in elements)
            {
                if (elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                // 要素の中心位置を計算
                if (!nodeById.TryGetValue(elem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(elem.NodeIds[1], out Node n1)) continue;

                var p0 = new Point3d(n0.Position.X, n0.Position.Y, n0.Position.Z);
                var p1 = new Point3d(n1.Position.X, n1.Position.Y, n1.Position.Z);
                var center = (p0 + p1) * 0.5;

                // 局所座標系を取得または計算
                double[] ex, ey, ez;
                if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                {
                    // 未計算の場合は計算
                    elem.CalcLocalAxis(new List<Node> { n0, n1 });
                    if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                    {
                        // ❌ このブロックは実行されないはず（Model初期化時に計算済み）
                        continue;
                    }
                }

                // 矢印の長さ（要素長の一定割合）
                double elemLength = p0.DistanceTo(p1);
                double arrowLength = elemLength * LocalAxisScale;

                // 局所X軸の矢印（赤）
                var exVec = new Vector3d(ex[0], ex[1], ex[2]) * arrowLength;
                _localAxisArrows.Add(new LocalAxisArrow
                {
                    Start = center,
                    End = center + exVec,
                    Color = Color.Red,
                    Label = "X"
                });

                // 局所Y軸の矢印（緑）
                var eyVec = new Vector3d(ey[0], ey[1], ey[2]) * arrowLength;
                _localAxisArrows.Add(new LocalAxisArrow
                {
                    Start = center,
                    End = center + eyVec,
                    Color = Color.Green,
                    Label = "Y"
                });

                // 局所Z軸の矢印（青）
                var ezVec = new Vector3d(ez[0], ez[1], ez[2]) * arrowLength;
                _localAxisArrows.Add(new LocalAxisArrow
                {
                    Start = center,
                    End = center + ezVec,
                    Color = Color.Blue,
                    Label = "Z"
                });
            }
        }

        /// <summary>
        /// 断面形状の3Dモデルを生成
        /// </summary>
        private void GenerateCrossSectionBreps(List<ElementBase> elements, List<Node> nodes)
        {
            _crossSectionBreps.Clear();

            var nodeById = nodes.ToDictionary(n => n.Id, n => n);

            foreach (var elem in elements)
            {
                if (elem.NodeIds == null || elem.NodeIds.Count < 2)
                    continue;

                // LineElement（BeamElement等）のみ対応
                if (!(elem is LineElement))
                    continue;

                // 断面情報を取得
                var crossSection = elem.CrossSection as CrossSection_Beam;
                if (crossSection == null)
                    continue;

                // ノード取得
                if (!nodeById.TryGetValue(elem.NodeIds[0], out Node n0)) continue;
                if (!nodeById.TryGetValue(elem.NodeIds[1], out Node n1)) continue;

                var p0 = new Point3d(n0.Position.X, n0.Position.Y, n0.Position.Z);
                var p1 = new Point3d(n1.Position.X, n1.Position.Y, n1.Position.Z);

                // 局所座標系を取得または計算
                double[] ex, ey, ez;
                if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                {
                    elem.CalcLocalAxis(new List<Node> { n0, n1 });
                    if (!elem.TryGetLocalCoordinateSystem(out ex, out ey, out ez))
                    {
                        continue;
                    }
                }

                // 断面曲線を生成
                Curve sectionCurve = CreateCrossSectionCurve(crossSection, p0, ey, ez);
                if (sectionCurve == null)
                    continue;

                // スイープ用のレール（部材軸線）
                var rail = new LineCurve(p0, p1);

                // スイープでBrepを生成
                var sweepBreps = Brep.CreateFromSweep(rail, sectionCurve, false, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (sweepBreps != null && sweepBreps.Length > 0)
                {
                    foreach (var brep in sweepBreps)
                    {
                        if (brep != null && brep.IsValid)
                        {
                            _crossSectionBreps.Add(brep);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 断面形状の曲線を生成（局所座標系に配置）
        /// </summary>
        private Curve CreateCrossSectionCurve(CrossSection_Beam cs, Point3d origin, double[] localY, double[] localZ)
        {
            // 局所Y軸とZ軸のベクトル
            Vector3d yVec = new Vector3d(localY[0], localY[1], localY[2]);
            Vector3d zVec = new Vector3d(localZ[0], localZ[1], localZ[2]);

            // 断面の種類に応じて形状を生成
            if (cs is CrossSection_Box boxSection)
            {
                return CreateBoxSectionCurve(origin, yVec, zVec, boxSection);
            }
            else if (cs is CrossSection_H hSection)
            {
                return CreateHSectionCurve(origin, yVec, zVec, hSection);
            }
            else if (cs is CrossSection_Circle circle)
            {
                return CreateCircleCurve(origin, yVec, zVec, circle);
            }

            return null;
        }

        /// <summary>
        /// 円形断面曲線を生成（中空）
        /// </summary>
        private Curve CreateCircleCurve(Point3d origin, Vector3d yVec, Vector3d zVec, CrossSection_Circle circle)
        {
            // 局所Y-Z平面上に円を作成
            var xVec = Vector3d.CrossProduct(yVec, zVec);
            var plane = new Plane(origin, xVec);

            double outerRadius = circle.D / 2.0;
            double innerRadius = (circle.D - 2 * circle.t) / 2.0;

            // 外側の円
            var outerCircle = new Circle(plane, outerRadius);
            var outerCurve = outerCircle.ToNurbsCurve();

            // 内側の円（厚みがある場合）
            if (innerRadius > 0 && innerRadius < outerRadius)
            {
                var innerCircle = new Circle(plane, innerRadius);
                var innerCurve = innerCircle.ToNurbsCurve();

                // 外側と内側の曲線を結合して中空断面を作成
                var curves = new List<Curve> { outerCurve, innerCurve };
                var joinedCurves = Curve.JoinCurves(curves, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true);

                if (joinedCurves != null && joinedCurves.Length > 0)
                {
                    return joinedCurves[0];
                }
            }

            // 厚みが大きすぎて内側が作れない場合は外側のみ
            return outerCurve;
        }

        /// <summary>
        /// 角形鋼管断面曲線を生成（中空の矩形）
        /// </summary>
        private Curve CreateBoxSectionCurve(Point3d origin, Vector3d yVec, Vector3d zVec, CrossSection_Box box)
        {
            double B = box.B;  // 幅
            double H = box.H;  // 高さ
            double t = box.t;  // 板厚

            double halfB = B / 2.0;
            double halfH = H / 2.0;

            // 外側の矩形
            var outerP1 = origin - zVec * halfH - yVec * halfB;
            var outerP2 = origin - zVec * halfH + yVec * halfB;
            var outerP3 = origin + zVec * halfH + yVec * halfB;
            var outerP4 = origin + zVec * halfH - yVec * halfB;

            var outerPolyline = new Polyline(new[] { outerP1, outerP2, outerP3, outerP4, outerP1 });
            var outerCurve = outerPolyline.ToNurbsCurve();

            // 内側の矩形（板厚分小さい）
            double innerHalfB = halfB - t;
            double innerHalfH = halfH - t;

            if (innerHalfB > 0 && innerHalfH > 0)
            {
                var innerP1 = origin - zVec * innerHalfH - yVec * innerHalfB;
                var innerP2 = origin - zVec * innerHalfH + yVec * innerHalfB;
                var innerP3 = origin + zVec * innerHalfH + yVec * halfB;
                var innerP4 = origin + zVec * innerHalfH - yVec * innerHalfB;

                var innerPolyline = new Polyline(new[] { innerP1, innerP2, innerP3, innerP4, innerP1 });
                var innerCurve = innerPolyline.ToNurbsCurve();

                // 外側と内側の曲線を結合して中空断面を作成
                var curves = new List<Curve> { outerCurve, innerCurve };
                var joinedCurves = Curve.JoinCurves(curves, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true);
                
                if (joinedCurves != null && joinedCurves.Length > 0)
                {
                    return joinedCurves[0];
                }
            }

            // 板厚が大きすぎて内側が作れない場合は外側のみ
            return outerCurve;
        }

        /// <summary>
        /// H形鋼断面曲線を生成
        /// </summary>
        private Curve CreateHSectionCurve(Point3d origin, Vector3d yVec, Vector3d zVec, CrossSection_H h)
        {
            double H = h.H;
            double B = h.B;
            double tw = h.t_w;
            double tf = h.t_f;

            // H形鋼の輪郭（12点）
            double halfH = H / 2.0;
            double halfB = B / 2.0;
            double halfTw = tw / 2.0;
            double innerY = halfH - tf;

            var points = new List<Point3d>
            {
                // 下フランジ下端左
                origin - zVec * halfH - yVec * halfB,
                // 下フランジ下端右
                origin - zVec * halfH + yVec * halfB,
                // 下フランジ上端右
                origin - zVec * innerY + yVec * halfB,
                // ウェブ右
                origin - zVec * innerY + yVec * halfTw,
                // ウェブ右上
                origin + zVec * innerY + yVec * halfTw,
                // 上フランジ下端右
                origin + zVec * innerY + yVec * halfB,
                // 上フランジ上端右
                origin + zVec * halfH + yVec * halfB,
                // 上フランジ上端左
                origin + zVec * halfH - yVec * halfB,
                // 上フランジ下端左
                origin + zVec * innerY - yVec * halfB,
                // ウェブ左上
                origin + zVec * innerY - yVec * halfTw,
                // ウェブ左下
                origin - zVec * innerY - yVec * halfTw,
                // 下フランジ上端左
                origin - zVec * innerY - yVec * halfB
            };

            points.Add(points[0]); // 閉じる

            var polyline = new Polyline(points);
            return polyline.ToNurbsCurve();
        }

        #region Viewport Display

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cachedModel == null) return;

            var display = args.Display;

            // 要素の線材モデルを常に表示（濃い灰色）
            if (_elementLines != null && _elementLines.Count > 0)
            {
                foreach (var line in _elementLines)
                {
                    display.DrawLine(line, Color.FromArgb(60, 60, 60), 2);
                }
            }

            // NodeID表示
            if (ShowNodeId)
            {
                foreach (var kvp in _nodePositions)
                {
                    display.DrawDot(kvp.Value, kvp.Key.ToString(), Color.DarkBlue, Color.White);
                }
            }

            // ElementID表示
            if (ShowElementId)
            {
                foreach (var kvp in _elementCenters)
                {
                    display.DrawDot(kvp.Value, kvp.Key.ToString(), Color.DarkGreen, Color.White);
                }
            }

            // Load表示
            if (ShowLoad && _cachedModel.Loads != null)
            {
                DrawLoads(display, _cachedModel.Loads);
            }

            // Support表示
            if (ShowSupport && _cachedModel.Supports != null)
            {
                DrawSupports(display, args.Viewport, _cachedModel.Supports);
            }

            // LocalAxis表示
            if (ShowLocalAxis && _localAxisArrows != null)
            {
                DrawLocalAxisArrows(display);
            }

            // CrossSection表示（ワイヤーフレーム）
            if (ShowCrossSection && _crossSectionBreps != null)
            {
                DrawCrossSectionWires(display);
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // CrossSection表示（シェーディング）
            if (ShowCrossSection && _crossSectionBreps != null)
            {
                DrawCrossSectionMeshes(args.Display);
            }
        }

        /// <summary>
        /// 局所座標系の矢印を描画
        /// </summary>
        private void DrawLocalAxisArrows(Rhino.Display.DisplayPipeline display)
        {
            foreach (var arrow in _localAxisArrows)
            {
                // 矢印を描画
                var line = new Line(arrow.Start, arrow.End);
                display.DrawArrow(line, arrow.Color, 15, 10);

                // ラベルを矢印の先端に表示
                var labelPos = arrow.End + (arrow.End - arrow.Start) * 0.1;
                display.DrawDot(labelPos, arrow.Label, arrow.Color, Color.White);
            }
        }

        /// <summary>
        /// 断面形状のワイヤーフレームを描画
        /// </summary>
        private void DrawCrossSectionWires(Rhino.Display.DisplayPipeline display)
        {
            foreach (var brep in _crossSectionBreps)
            {
                if (brep != null && brep.IsValid)
                {
                    display.DrawBrepWires(brep, Color.DarkGray, 1);
                }
            }
        }

        /// <summary>
        /// 断面形状のシェーディングを描画
        /// </summary>
        private void DrawCrossSectionMeshes(Rhino.Display.DisplayPipeline display)
        {
            var material = new Rhino.Display.DisplayMaterial(Color.LightGray, 0.5);

            foreach (var brep in _crossSectionBreps)
            {
                if (brep != null && brep.IsValid)
                {
                    display.DrawBrepShaded(brep, material);
                }
            }
        }

        /// <summary>
        /// 荷重を描画
        /// </summary>
        private void DrawLoads(Rhino.Display.DisplayPipeline display, List<Load> loads)
        {
            foreach (var load in loads)
            {
                if (load is PointLoad pointLoad)
                {
                    DrawPointLoad(display, pointLoad);
                }
                else if (load is ElementLoad elementLoad)
                {
                    DrawElementLoad(display, elementLoad);
                }
            }
        }

        /// <summary>
        /// 節点荷重を矢印で描画
        /// </summary>
        private void DrawPointLoad(Rhino.Display.DisplayPipeline display, PointLoad load)
        {
            if (!_nodePositions.ContainsKey(load.NodeId)) return;

            var pt = _nodePositions[load.NodeId];
            var force = load.Force;
            var moment = load.Moment;

            // 力の大きさ
            double fMag = Math.Sqrt(force.X * force.X + force.Y * force.Y + force.Z * force.Z);
            if (fMag > 1e-6)
            {
                // スケール調整(適宜変更)
                double scale = 0.1;
                var forceVec = new Vector3d(force.X, force.Y, force.Z);
                forceVec.Unitize();
                forceVec *= scale;

                var arrowEnd = pt + forceVec;
                display.DrawArrow(new Line(pt, arrowEnd), Color.Red, 10, 5);

                // 荷重値を小数第1位まで表示（指数表示なし）
                var labelOffset = forceVec * 0.2;
                var labelPt = arrowEnd + labelOffset;
                display.DrawDot(labelPt, $"{fMag:F1}", Color.Red, Color.White);
            }

            // モーメントの表示(簡易的に円で表現)
            double mMag = Math.Sqrt(moment.X * moment.X + moment.Y * moment.Y + moment.Z * moment.Z);
            if (mMag > 1e-6)
            {
                var circle = new Circle(pt, 0.05);
                display.DrawCircle(circle, Color.Orange, 2);
                display.DrawDot(pt + new Vector3d(0, 0, 0.07), $"{mMag:F1}", Color.Orange, Color.White);
            }
        }

        /// <summary>
        /// 要素荷重を描画
        /// </summary>
        private void DrawElementLoad(Rhino.Display.DisplayPipeline display, ElementLoad load)
        {
            var elem = _cachedModel.Elements.FirstOrDefault(e => e.Id == load.ElementId);
            if (elem == null || elem.NodeIds == null || elem.NodeIds.Count < 2) return;

            var n0Id = elem.NodeIds[0];
            var n1Id = elem.NodeIds[1];
            if (!_nodePositions.ContainsKey(n0Id) || !_nodePositions.ContainsKey(n1Id)) return;

            var p0 = _nodePositions[n0Id];
            var p1 = _nodePositions[n1Id];
            var center = (p0 + p1) * 0.5;

            var q = load.QLocal;
            double qMag = Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
            if (qMag > 1e-6)
            {
                // 簡易的に中点に矢印表示
                double scale = 0.1;
                var qVec = new Vector3d(q.X, q.Y, q.Z);
                qVec.Unitize();
                qVec *= scale;

                var arrowEnd = center + qVec;
                display.DrawArrow(new Line(center, arrowEnd), Color.Magenta, 8, 4);

                // 荷重値を小数第1位まで表示（指数表示なし）
                var labelOffset = qVec * 0.2;
                var labelPt = arrowEnd + labelOffset;
                display.DrawDot(labelPt, $"{qMag:F1}", Color.Magenta, Color.White);
            }
        }

        /// <summary>
        /// 支持条件を六角形記号で描画
        /// </summary>
        private void DrawSupports(Rhino.Display.DisplayPipeline display, Rhino.Display.RhinoViewport viewport, List<Support> supports)
        {
            foreach (var support in supports)
            {
                if (!_nodePositions.ContainsKey(support.NodeId)) continue;

                var pt = _nodePositions[support.NodeId];
                DrawSupportSymbol(display, viewport, pt, support.Conditions);
            }
        }

        /// <summary>
        /// 支持条件の六角形記号を描画(画面座標ベース、ビュー正対)
        /// 六角形を6つの三角形に分割: UX, UY, UZ, RX, RY, RZ
        /// </summary>
        private void DrawSupportSymbol(Rhino.Display.DisplayPipeline display, Rhino.Display.RhinoViewport viewport, Point3d worldCenter, bool[] conditions)
        {
            if (conditions == null || conditions.Length < 6) return;

            // 黒い枠線、荷重表示と同じ赤色の塗りつぶし
            Color outlineColor = Color.DarkRed;
            Color fillColor = Color.Red; // 荷重の矢印と同じ赤色

            // 画面上での一定サイズ(ピクセル) - 50%に縮小
            int screenRadius = 10; // 20から10に変更

            // ワールド座標を画面座標に変換
            var screenCenterPoint = viewport.WorldToClient(worldCenter);
            double screenCenterX = screenCenterPoint.X;
            double screenCenterY = screenCenterPoint.Y;

            // カメラ方向と中心点からビューに正対した平面を構築
            Vector3d cameraDir = viewport.CameraDirection;
            Plane viewPlane = new Plane(worldCenter, cameraDir);

            // 各頂点を画面座標からビュー平面上のワールド座標に変換
            Point3d[] worldHexPoints = new Point3d[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 2 - i * Math.PI / 3; // 右上から時計回り
                int screenX = (int)(screenCenterX + screenRadius * Math.Cos(angle));
                int screenY = (int)(screenCenterY - screenRadius * Math.Sin(angle)); // Y軸は画面上で下向きが正

                Line ray = viewport.ClientToWorld(new System.Drawing.Point(screenX, screenY));
                double t;
                if (Rhino.Geometry.Intersect.Intersection.LinePlane(ray, viewPlane, out t))
                {
                    worldHexPoints[i] = ray.PointAt(t);
                }
                else
                {
                    worldHexPoints[i] = worldCenter; // フォールバック
                }
            }

            // 六角形の輪郭を描画(黒)
            var hexPoly = new Polyline(worldHexPoints);
            hexPoly.Add(worldHexPoints[0]);
            display.DrawPolyline(hexPoly, outlineColor, 2);

            // 中心から各頂点への分割線を描画(黒)
            for (int i = 0; i < 6; i++)
            {
                display.DrawLine(worldCenter, worldHexPoints[i], outlineColor, 1);
            }

            // 各三角形を描画(固定条件の場合は赤で塗りつぶし)
            for (int i = 0; i < 6; i++)
            {
                if (conditions[i])
                {
                    int next = (i + 1) % 6;
                    var mesh = new Mesh();
                    mesh.Vertices.Add(worldCenter);
                    mesh.Vertices.Add(worldHexPoints[i]);
                    mesh.Vertices.Add(worldHexPoints[next]);
                    mesh.Faces.AddFace(0, 1, 2);
                    mesh.Normals.ComputeNormals();

                    display.DrawMeshShaded(mesh, new Rhino.Display.DisplayMaterial(fillColor));
                }
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                if (_nodePositions.Count == 0) return BoundingBox.Empty;

                var bbox = BoundingBox.Empty;
                foreach (var pt in _nodePositions.Values)
                {
                    bbox.Union(pt);
                }

                // 局所座標系の矢印も含める
                if (ShowLocalAxis && _localAxisArrows != null)
                {
                    foreach (var arrow in _localAxisArrows)
                    {
                        bbox.Union(arrow.Start);
                        bbox.Union(arrow.End);
                    }
                }

                // 断面形状も含める
                if (ShowCrossSection && _crossSectionBreps != null)
                {
                    foreach (var brep in _crossSectionBreps)
                    {
                        if (brep != null && brep.IsValid)
                        {
                            bbox.Union(brep.GetBoundingBox(false));
                        }
                    }
                }

                // 余裕を持たせる
                bbox.Inflate(1.0);
                return bbox;
            }
        }

        /// <summary>
        /// データの書き込み（保存時）
        /// </summary>
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("ShowNodeId", ShowNodeId);
            writer.SetBoolean("ShowElementId", ShowElementId);
            writer.SetBoolean("ShowLoad", ShowLoad);
            writer.SetBoolean("ShowSupport", ShowSupport);
            writer.SetBoolean("ShowLocalAxis", ShowLocalAxis);
            writer.SetBoolean("ShowCrossSection", ShowCrossSection);
            writer.SetDouble("LocalAxisScale", LocalAxisScale);
            writer.SetBoolean("IsDisplayTabExpanded", IsDisplayTabExpanded);
            return base.Write(writer);
        }

        /// <summary>
        /// データの読み込み（読込時）
        /// </summary>
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ShowNodeId"))
                ShowNodeId = reader.GetBoolean("ShowNodeId");
            if (reader.ItemExists("ShowElementId"))
                ShowElementId = reader.GetBoolean("ShowElementId");
            if (reader.ItemExists("ShowLoad"))
                ShowLoad = reader.GetBoolean("ShowLoad");
            if (reader.ItemExists("ShowSupport"))
                ShowSupport = reader.GetBoolean("ShowSupport");
            if (reader.ItemExists("ShowLocalAxis"))
                ShowLocalAxis = reader.GetBoolean("ShowLocalAxis");
            if (reader.ItemExists("ShowCrossSection"))
                ShowCrossSection = reader.GetBoolean("ShowCrossSection");
            if (reader.ItemExists("LocalAxisScale"))
                LocalAxisScale = reader.GetDouble("LocalAxisScale");
            if (reader.ItemExists("IsDisplayTabExpanded"))
                IsDisplayTabExpanded = reader.GetBoolean("IsDisplayTabExpanded");
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("07CEABD2-D9DD-4C6A-ACFD-40EEFB58B622"); }
        }
    }

    /// <summary>
    /// 局所座標系矢印の情報を保持
    /// </summary>
    internal class LocalAxisArrow
    {
        public Point3d Start { get; set; }
        public Point3d End { get; set; }
        public Color Color { get; set; }
        public string Label { get; set; }
    }
}
    #endregion