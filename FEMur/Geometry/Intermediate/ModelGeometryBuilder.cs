using System;
using System.Collections.Generic;
using System.Linq;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.CrossSections;

namespace FEMur.Geometry.Intermediate
{
    /// <summary>
    /// GeometryLineからFEMur構造解析モデルへの変換を管理するビルダークラス
    /// ノードの重複排除と自動ID採番を行う
    /// </summary>
    public class ModelGeometryBuilder
    {
        private readonly List<GeometryNode> _geometryNodes;
        private readonly List<GeometryLine> _geometryLines;
        private readonly Dictionary<GeometryNode, int> _nodeToIdMap;
        private readonly List<Node> _nodes;
        private readonly List<ElementBase> _elements;
        private double _nodeTolerance;

        /// <summary>
        /// ノード重複判定の許容誤差（デフォルト：1e-6）
        /// </summary>
        public double NodeTolerance
        {
            get => _nodeTolerance;
            set => _nodeTolerance = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelGeometryBuilder(double nodeTolerance = 1e-6)
        {
            _geometryNodes = new List<GeometryNode>();
            _geometryLines = new List<GeometryLine>();
            _nodeToIdMap = new Dictionary<GeometryNode, int>();
            _nodes = new List<Node>();
            _elements = new List<ElementBase>();
            _nodeTolerance = nodeTolerance;
        }

        /// <summary>
        /// GeometryLineを追加
        /// </summary>
        public void AddLine(GeometryLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _geometryLines.Add(line);
        }

        /// <summary>
        /// 複数のGeometryLineを追加
        /// </summary>
        public void AddLines(IEnumerable<GeometryLine> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            foreach (var line in lines)
            {
                AddLine(line);
            }
        }

        /// <summary>
        /// GeometryNodeを追加（明示的に追加する場合）
        /// </summary>
        public void AddNode(GeometryNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!_geometryNodes.Contains(node))
            {
                _geometryNodes.Add(node);
            }
        }

        /// <summary>
        /// ノードを統合し、重複を排除してNodeリストを生成
        /// </summary>
        private void BuildNodes()
        {
            _nodes.Clear();
            _nodeToIdMap.Clear();

            // 全ラインからノードを収集
            var allNodes = new List<GeometryNode>();
            foreach (var line in _geometryLines)
            {
                allNodes.Add(line.StartNode);
                allNodes.Add(line.EndNode);
            }
            // 明示的に追加されたノードも追加
            allNodes.AddRange(_geometryNodes);

            // 重複排除（許容誤差を考慮）
            var uniqueNodes = new List<GeometryNode>();
            foreach (var node in allNodes)
            {
                bool isDuplicate = false;
                foreach (var existing in uniqueNodes)
                {
                    if (AreNodesEqual(node, existing, _nodeTolerance))
                    {
                        isDuplicate = true;
                        // マップに既存ノードのIDを登録
                        if (!_nodeToIdMap.ContainsKey(node))
                        {
                            _nodeToIdMap[node] = uniqueNodes.IndexOf(existing);
                        }
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    uniqueNodes.Add(node);
                    _nodeToIdMap[node] = uniqueNodes.Count - 1;
                }
            }

            // FEMur.Nodes.Nodeに変換
            for (int i = 0; i < uniqueNodes.Count; i++)
            {
                _nodes.Add(uniqueNodes[i].ToNode(i));
            }
        }

        /// <summary>
        /// 2つのGeometryNodeが許容誤差内で一致するか判定
        /// </summary>
        private bool AreNodesEqual(GeometryNode node1, GeometryNode node2, double tolerance)
        {
            var dx = node1.Position.X - node2.Position.X;
            var dy = node1.Position.Y - node2.Position.Y;
            var dz = node1.Position.Z - node2.Position.Z;
            var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return distance <= tolerance;
        }

        /// <summary>
        /// BeamElementリストを生成
        /// </summary>
        public List<BeamElement> BuildBeamElements(Material defaultMaterial = null, CrossSection_Beam defaultCrossSection = null)
        {
            if (_nodes.Count == 0)
            {
                BuildNodes();
            }

            _elements.Clear();

            for (int i = 0; i < _geometryLines.Count; i++)
            {
                var line = _geometryLines[i];

                // 材料と断面の決定
                var material = line.Material ?? defaultMaterial;
                var crossSection = line.CrossSection ?? defaultCrossSection;

                if (material == null)
                {
                    throw new InvalidOperationException($"Line {i}: Material is not specified.");
                }
                if (crossSection == null)
                {
                    throw new InvalidOperationException($"Line {i}: CrossSection is not specified.");
                }

                // ノードIDを取得
                int startNodeId = GetNodeId(line.StartNode);
                int endNodeId = GetNodeId(line.EndNode);

                // BeamElementを生成
                var element = new BeamElement(
                    id: i,
                    node1Id: startNodeId,
                    node2Id: endNodeId,
                    material: material,
                    crossSection: crossSection,
                    betaAngle: line.BetaAngle
                );

                _elements.Add(element);
            }

            return _elements.Cast<BeamElement>().ToList();
        }

        /// <summary>
        /// GeometryNodeに対応するNode IDを取得
        /// </summary>
        private int GetNodeId(GeometryNode geometryNode)
        {
            // まず完全一致を探す
            if (_nodeToIdMap.TryGetValue(geometryNode, out int id))
            {
                return id;
            }

            // 許容誤差内で一致するノードを探す
            foreach (var kvp in _nodeToIdMap)
            {
                if (AreNodesEqual(geometryNode, kvp.Key, _nodeTolerance))
                {
                    return kvp.Value;
                }
            }

            throw new InvalidOperationException($"GeometryNode not found in node map: {geometryNode}");
        }

        /// <summary>
        /// 生成されたNodeリストを取得
        /// </summary>
        public List<Node> GetNodes()
        {
            if (_nodes.Count == 0)
            {
                BuildNodes();
            }
            return new List<Node>(_nodes);
        }

        /// <summary>
        /// ビルダーの状態をクリア
        /// </summary>
        public void Clear()
        {
            _geometryNodes.Clear();
            _geometryLines.Clear();
            _nodeToIdMap.Clear();
            _nodes.Clear();
            _elements.Clear();
        }

        /// <summary>
        /// ビルダーの統計情報を取得
        /// </summary>
        public string GetStatistics()
        {
            return $"GeometryNodes: {_geometryNodes.Count}, " +
                   $"GeometryLines: {_geometryLines.Count}, " +
                   $"Nodes: {_nodes.Count}, " +
                   $"Elements: {_elements.Count}";
        }
    }
}
