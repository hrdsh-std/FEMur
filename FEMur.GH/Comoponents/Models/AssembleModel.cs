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

namespace FEMurGH.Comoponents.Models
{
    /// <summary>
    /// AssembleModel コンポーネント - FEMur モデルの組み立て
    /// </summary>
    public partial class AssembleModel : GH_Component
    {
        #region Properties

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

        #endregion

        #region Constructor and Component Info

        public AssembleModel()
          : base("AssembleModel(FEMur)", "AssembleModel",
              "Assemble FEMur Model from Elements, Supports and Loads (Nodes are auto-generated from Elements)",
              "FEMur", "7.Model")
        {
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("07CEABD2-D9DD-4C6A-ACFD-40EEFB58B622");

        #endregion
           
        #region Input/Output Parameters

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (List<FEMur.Elements.ElementBase>) - Nodes will be auto-generated from element Points", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "FEMur Supports (List<FEMur.Supports.Support>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "L", "FEMur Loads (List<FEMur.Loads.Load>)", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "FEMur Model", GH_ParamAccess.item);
        }

        #endregion

        #region Solve Instance

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

            DA.GetDataList(1, supports);
            DA.GetDataList(2, loads);

            try
            {
                ResetCachedIds(elements, supports, loads);
                var model = new Model(new List<Node>(), elements, supports, loads);
                _cachedModel = model;

                UpdateCaches(model);

                DA.SetData(0, model);
            }
            catch (InvalidOperationException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// キャッシュされたIDをリセット（Grasshopperのオブジェクト再利用対策）
        /// </summary>
        private void ResetCachedIds(List<ElementBase> elements, List<Support> supports, List<Load> loads)
        {
            foreach (var element in elements)
            {
                if (element.NodeIds != null && element.NodeIds.Count > 0 && 
                    element.Points != null && element.Points.Count > 0)
                {
                    element.NodeIds.Clear();
                }
            }

            foreach (var support in supports)
            {
                if (support.Position.HasValue)
                {
                    support.NodeId = -1;
                }
            }

            foreach (var load in loads)
            {
                if (load is PointAction pointAction && pointAction.Position.HasValue)
                {
                    pointAction.NodeId = -1;
                }
            }
        }

        /// <summary>
        /// キャッシュを更新
        /// </summary>
        private void UpdateCaches(Model model)
        {
            CachePositions(model.Nodes, model.Elements);
            GenerateElementLines(model.Elements, model.Nodes);

            if (ShowLocalAxis)
            {
                GenerateLocalAxisArrows(model.Elements, model.Nodes);
            }
            else
            {
                _localAxisArrows.Clear();
            }

            if (ShowCrossSection)
            {
                GenerateCrossSectionBreps(model.Elements, model.Nodes);
            }
            else
            {
                _crossSectionBreps.Clear();
            }
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

        #endregion


        #region Custom Attributes

        public override void CreateAttributes()
        {
            m_attributes = new AssembleModelAttributes(this);
        }

        #endregion

        #region Serialization

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

        #endregion
    }
}