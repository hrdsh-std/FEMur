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

namespace FEMurGH.Models
{
    public class AssembleModel : GH_Component
    {
        // 表示設定
        public bool ShowNodeId { get; set; } = false;
        public bool ShowElementId { get; set; } = false;
        public bool ShowLoad { get; set; } = false;
        public bool ShowSupport { get; set; } = false;

        // キャッシュ
        private Model _cachedModel = null;
        private Dictionary<int, Point3d> _nodePositions = new Dictionary<int, Point3d>();
        private Dictionary<int, Point3d> _elementCenters = new Dictionary<int, Point3d>();

        /// <summary>
        /// Initializes a new instance of the AssembleModel class.
        /// </summary>
        public AssembleModel()
          : base("AssembleModel(FEMur)", "AssembleModel",
              "Assemble FEMur Model from Nodes, Elements, Supports and Loads",
              "FEMur", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "FEMur Nodes (List<FEMur.Nodes.Node>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (List<FEMur.Elements.ElementBase>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "FEMur Supports (List<FEMur.Supports.Support>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "L", "FEMur Loads (List<FEMur.Loads.Load>)", GH_ParamAccess.list);
            
            // Supports と Loads はオプショナル(空リストでも解析可能)
            pManager[2].Optional = true;
            pManager[3].Optional = true;
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
            var nodes = new List<Node>();
            var elements = new List<ElementBase>();
            var supports = new List<Support>();
            var loads = new List<Load>();

            if (!DA.GetDataList(0, nodes))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Nodes are required");
                return;
            }

            if (!DA.GetDataList(1, elements))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Elements are required");
                return;
            }

            // オプショナル入力
            DA.GetDataList(2, supports);
            DA.GetDataList(3, loads);

            // Model を作成
            var model = new Model(nodes, elements, supports, loads);
            _cachedModel = model;

            // 表示用の位置情報をキャッシュ
            CachePositions(nodes, elements);

            DA.SetData(0, model);
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

        #region Viewport Display

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cachedModel == null) return;

            var display = args.Display;

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

                // 荷重値を矢印の先端から少しずらした位置に表示
                var labelOffset = forceVec * 0.2; // 矢印方向に少しオフセット
                var labelPt = arrowEnd + labelOffset;
                display.DrawDot(labelPt, $"{fMag:F2}", Color.Red, Color.White);
            }

            // モーメントの表示(簡易的に円で表現)
            double mMag = Math.Sqrt(moment.X * moment.X + moment.Y * moment.Y + moment.Z * moment.Z);
            if (mMag > 1e-6)
            {
                var circle = new Circle(pt, 0.05);
                display.DrawCircle(circle, Color.Orange, 2);
                display.DrawDot(pt + new Vector3d(0, 0, 0.07), $"{mMag:F2}", Color.Orange, Color.White);
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
                
                // 荷重値を矢印の先端から少しずらした位置に表示
                var labelOffset = qVec * 0.2; // 矢印方向に少しオフセット
                var labelPt = arrowEnd + labelOffset;
                display.DrawDot(labelPt, $"{qMag:F2}", Color.Magenta, Color.White);
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
                // 余裕を持たせる
                bbox.Inflate(1.0);
                return bbox;
            }
        }

        #endregion

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
}