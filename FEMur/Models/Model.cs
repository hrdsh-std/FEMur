using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;
using FEMur.Geometry;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.Supports;
using FEMur.Loads;
using FEMur.Results;
using FEMur.Joints;

namespace FEMur.Models
{
    public class Model:CommonObject,ISerializable, IEquatable<Model>
    {
        public List<Node> Nodes { get; set; }
        public List<ElementBase> Elements { get; set; }
        public List<Support> Supports { get; set; }
        public List<Load> Loads { get; set; }
        public List<Joint> Joints { get; set; }

        // 追加: 解析結果を保持
        public Result Result { get; set; }

        // 追加: 計算済みフラグ
        public bool IsSolved { get; set; }

        // ノード自動生成時の許容誤差
        private const double NodeTolerance = 0.001;

        public Model()
        {
            Nodes = new List<Node>();
            Elements = new List<ElementBase>();
            Supports = new List<Support>();
            Loads = new List<Load>();
            Joints = new List<Joint>();
            Result = null;
            IsSolved = false;
        }

        public Model
            (List<Node> nodes, List<ElementBase> elements, 
            List<Support> supports,
            List<Load> loads, List<Joint> joints = null)
        {
            Nodes = nodes ?? new List<Node>();
            Elements = elements ?? new List<ElementBase>();
            Supports = supports ?? new List<Support>();
            Loads = loads ?? new List<Load>();
            Joints = joints ?? new List<Joint>();
            Result = null;
            IsSolved = false;
            
            // モデルの検証（自動ノード生成を含む）
            ValidateAndRepairModel();
            
            // 要素座標系の設定（必ずCalcLocalAxisが実行される）
            ElementCoordinateSystemHelper.SetupElementCoordinateSystems(this);  // ✅ ここで計算済み
        }

        public Model(Model other)
        {
            // LINQ でディープコピー
            this.Nodes = other.Nodes.Select(n => (Node)n.DeepCopy()).ToList();
            this.Elements = other.Elements.Select(e => (ElementBase)e.DeepCopy()).ToList();
            this.Supports = other.Supports.Select(s => (Support)s.DeepCopy()).ToList();
            this.Loads = other.Loads.Select(l => (Load)l.DeepCopy()).ToList();
            this.Joints = other.Joints?.Select(j => (Joint)j.DeepCopy()).ToList() ?? new List<Joint>();

            this.Result = null;
            this.IsSolved = false;

            // モデルの検証（自動ノード生成を含む）
            ValidateAndRepairModel();
            
            // 要素座標系の設定
            ElementCoordinateSystemHelper.SetupElementCoordinateSystems(this);
        }
        public override object DeepCopy()
        {
            return new Model(this);
        }

        public Model(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Nodes = (List<Node>)info.GetValue("Nodes", typeof(List<Node>));
            Elements = (List<ElementBase>)info.GetValue("Elements", typeof(List<ElementBase>));
            Supports = (List<Support>)info.GetValue("Supports", typeof(List<Support>));
            Loads = (List<Load>)info.GetValue("Loads", typeof(List<Load>));
            
            // シリアライゼーション時の互換性のため、nullチェック
            try
            {
                Joints = (List<Joint>)info.GetValue("Joints", typeof(List<Joint>));
            }
            catch
            {
                Joints = new List<Joint>();
            }
            
            try
            {
                Result = (Result)info.GetValue("Result", typeof(Result));
                IsSolved = info.GetBoolean("IsSolved");
            }
            catch
            {
                Result = null;
                IsSolved = false;
            }
            
            // モデルの検証（自動ノード生成を含む）
            ValidateAndRepairModel();
            
            // 要素座標系の設定
            ElementCoordinateSystemHelper.SetupElementCoordinateSystems(this);
        }

        /// <summary>
        /// モデルの整合性を検証し、必要に応じて修復（ノード自動生成・ID自動採番）
        /// </summary>
        /// <exception cref="InvalidOperationException">修復不可能な不整合がある場合</exception>
        private void ValidateAndRepairModel()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Nodesがnullの場合は初期化
            if (Nodes == null)
            {
                Nodes = new List<Node>();
            }

            if (Elements == null)
            {
                Elements = new List<ElementBase>();
            }

            if (Joints == null)
            {
                Joints = new List<Joint>();
            }

            // === ノードID自動採番 ===
            int nextNodeId = 0;
            var nodeByCoordinate = new Dictionary<Point3, Node>(new Point3Comparer(NodeTolerance));
            var nodeById = new Dictionary<int, Node>();

            // 既存ノードのIDチェックと再採番
            foreach (var node in Nodes)
            {
                if (node.Id < 0 || nodeById.ContainsKey(node.Id))
                {
                    node.Id = nextNodeId++;
                    warnings.Add($"Node at ({node.Position.X:F3}, {node.Position.Y:F3}, {node.Position.Z:F3}): Auto-assigned ID = {node.Id}");
                }
                else
                {
                    if (node.Id >= nextNodeId)
                    {
                        nextNodeId = node.Id + 1;
                    }
                }

                nodeById[node.Id] = node;

                if (!nodeByCoordinate.ContainsKey(node.Position))
                {
                    nodeByCoordinate[node.Position] = node;
                }
                else
                {
                    warnings.Add($"Node ID {node.Id}: Duplicate coordinate with Node ID {nodeByCoordinate[node.Position].Id}");
                }
            }

            // === 要素の検証と修復（ノード自動生成）=== 
            // ★ Support/Load/Joint処理の前に移動
            if (Elements != null)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    var element = Elements[i];

                    if (element.NodeIds == null || element.NodeIds.Count == 0)
                    {
                        if (element.Points != null && element.Points.Count > 0)
                        {
                            element.NodeIds = new List<int>();

                            foreach (var point in element.Points)
                            {
                                Node node;

                                if (nodeByCoordinate.TryGetValue(point, out node))
                                {
                                    element.NodeIds.Add(node.Id);
                                }
                                else
                                {
                                    node = new Node(nextNodeId++, point.X, point.Y, point.Z);
                                    Nodes.Add(node);
                                    nodeByCoordinate[point] = node;
                                    nodeById[node.Id] = node;
                                    element.NodeIds.Add(node.Id);
                                }
                            }

                            warnings.Add($"Element[{i}]: Auto-generated {element.NodeIds.Count} nodes from Point3 coordinates.");
                        }
                        else
                        {
                            errors.Add($"Element[{i}]: NodeIds is null and Points is empty. Cannot create element without node information.");
                            continue;
                        }
                    }

                    if (element.NodeIds.Count == 0)
                    {
                        errors.Add($"Element[{i}]: NodeIds is empty.");
                        continue;
                    }

                    if (element is LineElement && element.NodeIds.Count < 2)
                    {
                        errors.Add($"Element[{i}]: LineElement requires at least 2 nodes, but has {element.NodeIds.Count}.");
                        continue;
                    }

                    foreach (var nodeId in element.NodeIds)
                    {
                        if (!nodeById.ContainsKey(nodeId))
                        {
                            errors.Add($"Element[{i}]: References non-existent Node ID {nodeId}.");
                        }
                    }
                }

                // === 要素ID自動採番 ===
                int nextElementId = 0;
                var usedElementIds = new HashSet<int>();

                for (int i = 0; i < Elements.Count; i++)
                {
                    var element = Elements[i];

                    if (element.Id < 0 || usedElementIds.Contains(element.Id))
                    {
                        element.Id = nextElementId++;
                        warnings.Add($"Element[{i}]: Auto-assigned ID = {element.Id}");
                    }
                    else
                    {
                        if (element.Id >= nextElementId)
                        {
                            nextElementId = element.Id + 1;
                        }
                    }

                    usedElementIds.Add(element.Id);
                }
            }

            // === Joints の ElementId 自動更新 ===
            // ★ Element処理の後に移動（要素IDが確定している）
            if (Joints != null && Joints.Count > 0)
            {
                for (int i = 0; i < Joints.Count; i++)
                {
                    var joint = Joints[i];

                    // 要素オブジェクトからElementIdを更新
                    joint.UpdateElementIdFromElement();

                    // ElementIdが-1の場合は警告
                    if (joint.ElementId < 0)
                    {
                        warnings.Add($"Joint[{i}]: ElementId is not set. Joint will be ignored during analysis.");
                    }
                }
            }

            // === Support の Point3 処理 ===
            // ★ Element処理の後に移動（要素からのノード生成が完了している）
            if (Supports != null)
            {
                for (int i = 0; i < Supports.Count; i++)
                {
                    var support = Supports[i];

                    // Point3で指定されている場合
                    if (support.Position.HasValue && support.NodeId < 0)
                    {
                        Node node;
                        var position = support.Position.Value;

                        // 既存ノードの中に同じ座標があるか確認
                        if (nodeByCoordinate.TryGetValue(position, out node))
                        {
                            // 既存ノードを使用
                            support.NodeId = node.Id;
                            warnings.Add($"Support[{i}]: Using existing Node ID {node.Id} at ({position.X:F3}, {position.Y:F3}, {position.Z:F3})");
                        }
                        else
                        {
                            // 既存ノードが見つからない場合はエラー
                            errors.Add($"Support[{i}]: No existing node found at position ({position.X:F3}, {position.Y:F3}, {position.Z:F3}). " +
                                     $"Support must reference an existing node created by elements. " +
                                     $"Tolerance: {NodeTolerance:E3}");
                        }
                    }
                }
            }

            // === PointLoad の Point3 処理 ===
            // ★ Element処理の後に移動（要素からのノード生成が完了している）
            if (Loads != null)
            {
                for (int i = 0; i < Loads.Count; i++)
                {
                    var load = Loads[i];

                    if (load is PointAction pointAction && pointAction.Position.HasValue && pointAction.NodeId < 0)
                    {
                        Node node;
                        var position = pointAction.Position.Value;

                        // 既存ノードの中に同じ座標があるか確認
                        if (nodeByCoordinate.TryGetValue(position, out node))
                        {
                            // 既存ノードを使用
                            pointAction.NodeId = node.Id;
                            warnings.Add($"Load[{i}] ({load.GetType().Name}): Using existing Node ID {node.Id} at ({position.X:F3}, {position.Y:F3}, {position.Z:F3})");
                        }
                        else
                        {
                            // 既存ノードが見つからない場合はエラー
                            errors.Add($"Load[{i}] ({load.GetType().Name}): No existing node found at position ({position.X:F3}, {position.Y:F3}, {position.Z:F3}). " +
                                     $"Load must reference an existing node created by elements. " +
                                     $"Tolerance: {NodeTolerance:E3}");
                        }
                    }
                }
            }

            // === 支持条件の検証 ===
            if (Supports != null && Nodes != null)
            {
                foreach (var support in Supports)
                {
                    if (!nodeById.ContainsKey(support.NodeId))
                    {
                        errors.Add($"Support references non-existent Node ID {support.NodeId}.");
                    }
                }
            }

            // === 荷重の検証 ===
            if (Loads != null)
            {
                var elementIds = Elements?.Select(e => e.Id).ToHashSet() ?? new HashSet<int>();

                foreach (var load in Loads)
                {
                    if (load is PointLoad pointLoad)
                    {
                        if (!nodeById.ContainsKey(pointLoad.NodeId))
                        {
                            errors.Add($"PointLoad references non-existent Node ID {pointLoad.NodeId}.");
                        }
                    }
                    else if (load is ElementLoad elementLoad)
                    {
                        if (!elementIds.Contains(elementLoad.ElementId))
                        {
                            errors.Add($"ElementLoad references non-existent Element ID {elementLoad.ElementId}.");
                        }
                    }
                }
            }

            // === Joints の検証 ===
            if (Joints != null && Joints.Count > 0)
            {
                var elementIds = Elements?.Select(e => e.Id).ToHashSet() ?? new HashSet<int>();

                for (int i = 0; i < Joints.Count; i++)
                {
                    var joint = Joints[i];

                    // ElementIdが-1の場合はスキップ（既に警告済み）
                    if (joint.ElementId < 0)
                    {
                        continue;
                    }

                    // 要素IDの検証
                    if (!elementIds.Contains(joint.ElementId))
                    {
                        errors.Add($"Joint[{i}]: References non-existent Element ID {joint.ElementId}.");
                    }
                }
            }

            // エラーがある場合は例外をスロー
            if (errors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Model validation failed:");
                foreach (var error in errors)
                {
                    errorMessage.AppendLine($"  - {error}");
                }

                if (warnings.Count > 0)
                {
                    errorMessage.AppendLine("\nWarnings:");
                    foreach (var warning in warnings)
                    {
                        errorMessage.AppendLine($"  - {warning}");
                    }
                }

                throw new InvalidOperationException(errorMessage.ToString());
            }

            // 警告のみの場合はコンソールに出力
            if (warnings.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Model validation warnings:");
                foreach (var warning in warnings)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {warning}");
                }
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Nodes", Nodes);
            info.AddValue("Elements", Elements);
            info.AddValue("Supports", Supports);
            info.AddValue("Loads", Loads);
            info.AddValue("Joints", Joints);
            info.AddValue("Result", Result);
            info.AddValue("IsSolved", IsSolved);
        }

        public override Object Clone()
        {
            var cloned = (Model)this.MemberwiseClone();
            // Deep copy for collections
            cloned.Nodes = new List<Node>(this.Nodes);
            cloned.Elements = new List<ElementBase>(this.Elements);
            cloned.Supports = new List<Support>(this.Supports);
            cloned.Loads = new List<Load>(this.Loads);
            cloned.Joints = new List<Joint>(this.Joints ?? new List<Joint>());
            
            // 要素座標系の設定
            ElementCoordinateSystemHelper.SetupElementCoordinateSystems(cloned);
            
            return cloned;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Model Summary:");
            sb.AppendLine($"Nodes: {Nodes.Count}");
            sb.AppendLine($"Elements: {Elements.Count}");
            sb.AppendLine($"Supports: {Supports.Count}");
            sb.AppendLine($"Loads: {Loads.Count}");
            sb.AppendLine($"Joints: {Joints?.Count ?? 0}");
            sb.AppendLine($"IsSolved: {IsSolved}");
            return sb.ToString();
        }

        public bool Equals(Model other)
        {
            if (other == null) return false;
            
            bool jointsEqual = (Joints == null && other.Joints == null) ||
                              (Joints != null && other.Joints != null && Joints.SequenceEqual(other.Joints));
            
            return Nodes.SequenceEqual(other.Nodes) &&
                   Elements.SequenceEqual(other.Elements) &&
                   Supports.SequenceEqual(other.Supports) &&
                   Loads.SequenceEqual(other.Loads) &&
                   jointsEqual;
        }
    }
}
