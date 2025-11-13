# 中間クラス設計：RhinoGeometry から FEMur 構造解析モデルへの変換

## 概要

FEMur 構造解析用モデルへの変換を容易にするため、中間表現クラスを設計しました。
これにより、以下の課題が解決されます：

- **Point3d と Element の変換のサポート**
- **Node ID を振らない構造への対応**
- **変換処理の拡張性・保守性の向上**

## 設計思想

### 問題点

従来の実装では：
1. Rhino の Point3d → FEMur の Node 変換時に明示的な ID 管理が必要
2. LineToBeam 処理で重複ノードの排除が困難
3. ジオメトリと構造要素の間に中間表現が存在しない

### 解決策

中間表現レイヤーを導入：
```
RhinoGeometry → 中間表現 (GeometryNode/Line) → FEMur Model (Node/Element)
```

## クラス構造

### 1. GeometryNode

位置情報のみを持つノード（ID 不要）

**プロパティ：**
- `Position: Point3` - ノードの位置座標
- `Tag: object` - オプショナルな参照情報（Rhino GUID など）

**メソッド：**
- `ToNode(int id): Node` - FEMur.Nodes.Node へ変換

**使用例：**
```csharp
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;

// 座標から直接生成
var node1 = new GeometryNode(0, 0, 0);
var node2 = new GeometryNode(1.5, 2.5, 3.5);

// Point3 から生成
var point = new Point3(10, 20, 30);
var node3 = new GeometryNode(point);

// FEMur Node に変換（ID を指定）
var femNode = node1.ToNode(0);
```

### 2. GeometryLine

2つの GeometryNode を結ぶ線要素

**プロパティ：**
- `StartNode: GeometryNode` - 始点ノード
- `EndNode: GeometryNode` - 終点ノード
- `Material: Material` - 材料特性（オプショナル）
- `CrossSection: CrossSection_Beam` - 断面特性（オプショナル）
- `BetaAngle: double` - β角（度、デフォルト: 0）
- `Tag: object` - オプショナルな参照情報

**読み取り専用プロパティ：**
- `Length: double` - 線の長さ
- `MidPoint: Point3` - 中点座標
- `Direction: Point3` - 方向ベクトル

**使用例：**
```csharp
// GeometryNode から生成
var startNode = new GeometryNode(0, 0, 0);
var endNode = new GeometryNode(5, 0, 0);
var line1 = new GeometryLine(startNode, endNode);

// Point3 から直接生成
var line2 = new GeometryLine(
    new Point3(0, 0, 0),
    new Point3(5, 0, 0)
);

// 材料と断面を設定
var material = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var crossSection = new CrossSection_Box(0, "Box100", 100, 100, 5);

line2.Material = material;
line2.CrossSection = crossSection;
line2.BetaAngle = 45.0;

// 線の情報を取得
Console.WriteLine($"Length: {line2.Length}");
Console.WriteLine($"MidPoint: {line2.MidPoint}");
```

### 3. ModelGeometryBuilder

GeometryLine から FEMur 構造解析モデルへの変換を管理

**主要機能：**
- ノードの自動重複排除（許容誤差内）
- 自動 ID 採番
- デフォルト材料・断面の適用

**プロパティ：**
- `NodeTolerance: double` - ノード重複判定の許容誤差（デフォルト: 1e-6）

**メソッド：**
- `AddLine(GeometryLine)` - 線を追加
- `AddLines(IEnumerable<GeometryLine>)` - 複数の線を追加
- `GetNodes(): List<Node>` - 統合された Node リストを取得
- `BuildBeamElements(...): List<BeamElement>` - BeamElement リストを生成
- `Clear()` - ビルダーの状態をクリア
- `GetStatistics(): string` - 統計情報を取得

**使用例：**
```csharp
using FEMur.Geometry.Intermediate;
using FEMur.Materials;
using FEMur.CrossSections;

// ビルダーを作成
var builder = new ModelGeometryBuilder(nodeTolerance: 1e-6);

// 材料と断面を定義
var material = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var crossSection = new CrossSection_Box(0, "Box100", 100, 100, 5);

// 複数の線を追加（接続点は自動的に統合される）
builder.AddLine(new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
{
    Material = material,
    CrossSection = crossSection
});

builder.AddLine(new GeometryLine(new Point3(1, 0, 0), new Point3(2, 0, 0))
{
    Material = material,
    CrossSection = crossSection
});

builder.AddLine(new GeometryLine(new Point3(2, 0, 0), new Point3(3, 0, 0))
{
    Material = material,
    CrossSection = crossSection
});

// ノードと要素を生成
var nodes = builder.GetNodes();          // 4個の一意なノード
var elements = builder.BuildBeamElements();  // 3個の要素

Console.WriteLine($"Nodes: {nodes.Count}");       // 4
Console.WriteLine($"Elements: {elements.Count}"); // 3
Console.WriteLine(builder.GetStatistics());
```

### 4. GeometryConverter

FEMur 内部のジオメトリ型と中間表現型の相互変換ユーティリティ

**メソッド：**
- `ToGeometryNode(Point3): GeometryNode`
- `ToGeometryNodes(IEnumerable<Point3>): List<GeometryNode>`
- `ToGeometryLine(Point3, Point3): GeometryLine`
- `ToGeometryLines(IEnumerable<(Point3, Point3)>): List<GeometryLine>`
- `ToPoint3(GeometryNode): Point3`
- `ToPoint3List(IEnumerable<GeometryNode>): List<Point3>`

**使用例：**
```csharp
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;

// Point3 → GeometryNode
var point = new Point3(1, 2, 3);
var node = GeometryConverter.ToGeometryNode(point);

// 複数の Point3 → GeometryNode リスト
var points = new List<Point3>
{
    new Point3(0, 0, 0),
    new Point3(1, 0, 0),
    new Point3(2, 0, 0)
};
var nodes = GeometryConverter.ToGeometryNodes(points);

// 線ペアから GeometryLine を生成
var linePairs = new List<(Point3, Point3)>
{
    (new Point3(0, 0, 0), new Point3(1, 0, 0)),
    (new Point3(1, 0, 0), new Point3(2, 0, 0))
};
var lines = GeometryConverter.ToGeometryLines(linePairs);
```

## 実用例

### 例1: 単純な梁モデル

```csharp
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;
using FEMur.Materials;
using FEMur.CrossSections;
using FEMur.Models;

// 材料と断面を定義
var steel = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var section = new CrossSection_Box(0, "Box100", 100, 100, 5);

// ビルダーを作成
var builder = new ModelGeometryBuilder();

// 5m の単純梁
var beam = new GeometryLine(new Point3(0, 0, 0), new Point3(5000, 0, 0))
{
    Material = steel,
    CrossSection = section
};

builder.AddLine(beam);

// モデルを生成
var nodes = builder.GetNodes();
var elements = builder.BuildBeamElements();

var model = new Model(nodes, elements.Cast<ElementBase>().ToList(), 
                      new List<Support>(), new List<Load>());
```

### 例2: トラス構造

```csharp
// デフォルト材料・断面を設定
var steel = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var section = new CrossSection_Box(0, "Box100", 100, 100, 5);

var builder = new ModelGeometryBuilder();

// トラスの節点座標
var node1 = new Point3(0, 0, 0);
var node2 = new Point3(5000, 0, 0);
var node3 = new Point3(10000, 0, 0);
var node4 = new Point3(2500, 3000, 0);
var node5 = new Point3(7500, 3000, 0);

// トラス部材（材料・断面は後で一括設定）
var members = new[]
{
    new GeometryLine(node1, node2),
    new GeometryLine(node2, node3),
    new GeometryLine(node1, node4),
    new GeometryLine(node2, node4),
    new GeometryLine(node2, node5),
    new GeometryLine(node3, node5),
    new GeometryLine(node4, node5)
};

builder.AddLines(members);

// デフォルト材料・断面で要素を生成
var nodes = builder.GetNodes();
var elements = builder.BuildBeamElements(
    defaultMaterial: steel,
    defaultCrossSection: section
);

// 統計情報を表示
Console.WriteLine(builder.GetStatistics());
// 出力: GeometryNodes: 0, GeometryLines: 7, Nodes: 5, Elements: 7
```

### 例3: Rhino から FEMur への変換（Grasshopper コンポーネント）

```csharp
// Grasshopper コンポーネント内での使用例
protected override void SolveInstance(IGH_DataAccess DA)
{
    var rhinoLines = new List<Rhino.Geometry.Line>();
    Material material = null;
    CrossSection_Beam crossSection = null;
    
    if (!DA.GetDataList(0, rhinoLines)) return;
    if (!DA.GetData(1, ref material)) return;
    if (!DA.GetData(2, ref crossSection)) return;
    
    // ModelGeometryBuilder で変換
    var builder = new ModelGeometryBuilder();
    
    foreach (var rhinoLine in rhinoLines)
    {
        // Rhino.Geometry.Point3d を FEMur.Geometry.Point3 に変換
        var start = new FEMur.Geometry.Point3(
            rhinoLine.From.X, rhinoLine.From.Y, rhinoLine.From.Z);
        var end = new FEMur.Geometry.Point3(
            rhinoLine.To.X, rhinoLine.To.Y, rhinoLine.To.Z);
        
        var geoLine = new GeometryLine(start, end)
        {
            Material = material,
            CrossSection = crossSection
        };
        
        builder.AddLine(geoLine);
    }
    
    // FEMur モデル要素を出力
    var nodes = builder.GetNodes();
    var elements = builder.BuildBeamElements();
    
    DA.SetDataList(0, nodes);
    DA.SetDataList(1, elements);
}
```

## 利点

### 1. ノード ID 管理の自動化

従来：
```csharp
// 手動で ID を管理する必要があった
var nodes = new List<Node>();
for (int i = 0; i < points.Count; i++)
{
    nodes.Add(new Node(i, points[i].X, points[i].Y, points[i].Z));
}
```

新方式：
```csharp
// ID は自動採番
var builder = new ModelGeometryBuilder();
builder.AddLines(geometryLines);
var nodes = builder.GetNodes();  // ID は自動的に 0, 1, 2, ...
```

### 2. 重複ノードの自動排除

従来：手動でノードの重複チェックが必要

新方式：
```csharp
// 許容誤差内で自動的に重複を排除
var builder = new ModelGeometryBuilder(nodeTolerance: 1e-6);
// 接続する線を追加しても、共有ノードは1つだけ作成される
```

### 3. 材料・断面の柔軟な設定

```csharp
// 各線に個別設定
line1.Material = steelMaterial;
line1.CrossSection = box100Section;

// または一括でデフォルト設定
var elements = builder.BuildBeamElements(
    defaultMaterial: steelMaterial,
    defaultCrossSection: box100Section
);
```

### 4. 拡張性

- `Tag` プロパティで任意の参照データを保持可能
- ジオメトリと構造解析の分離により、各レイヤーを独立して拡張可能

## テスト

包括的なユニットテストを実装済み：

- `GeometryNodeTest.cs` - GeometryNode の動作確認
- `GeometryLineTest.cs` - GeometryLine の動作確認
- `ModelGeometryBuilderTest.cs` - ビルダーの動作確認
- `GeometryConverterTest.cs` - 変換ユーティリティの動作確認

全テストが成功を確認済み。

## まとめ

中間クラスの導入により、以下を達成しました：

✅ Node ID 不要の構造で、ジオメトリから FEMur モデルへの変換が可能  
✅ 自動的なノード重複排除と ID 採番  
✅ 材料・断面の柔軟な設定  
✅ 拡張性の高い設計  
✅ 包括的なテストによる品質保証

これにより、LineToBeam 処理やその他の変換処理の保守性が大幅に向上しました。
