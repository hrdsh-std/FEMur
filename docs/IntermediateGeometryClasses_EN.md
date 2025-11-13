# Intermediate Geometry Classes for RhinoGeometry to FEMur Conversion

## Overview

This intermediate representation layer facilitates the conversion from RhinoGeometry to FEMur structural analysis models. It addresses the following key challenges:

- **Support for Point3d and Element conversion**
- **Support for structures without explicit Node IDs**
- **Improved extensibility and maintainability of conversion processes**

## Architecture

### Problem Statement

The previous implementation required:
1. Explicit ID management when converting Rhino Point3d to FEMur Nodes
2. Manual handling of duplicate node elimination in LineToBeam processing
3. Direct coupling between geometry and structural elements

### Solution

Introduced an intermediate representation layer:
```
RhinoGeometry → Intermediate (GeometryNode/Line) → FEMur Model (Node/Element)
```

## Class Design

### 1. GeometryNode

A node representation that only requires position information (no ID needed).

**Properties:**
- `Position: Point3` - Node position coordinates
- `Tag: object` - Optional reference data (e.g., Rhino GUID)

**Methods:**
- `ToNode(int id): Node` - Convert to FEMur.Nodes.Node

**Example:**
```csharp
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;

// Create from coordinates
var node1 = new GeometryNode(0, 0, 0);
var node2 = new GeometryNode(1.5, 2.5, 3.5);

// Create from Point3
var point = new Point3(10, 20, 30);
var node3 = new GeometryNode(point);

// Convert to FEMur Node (specify ID)
var femNode = node1.ToNode(0);
```

### 2. GeometryLine

A line element connecting two GeometryNodes.

**Properties:**
- `StartNode: GeometryNode` - Start node
- `EndNode: GeometryNode` - End node
- `Material: Material` - Material properties (optional)
- `CrossSection: CrossSection_Beam` - Cross section properties (optional)
- `BetaAngle: double` - Beta angle in degrees (default: 0)
- `Tag: object` - Optional reference data

**Read-only Properties:**
- `Length: double` - Line length
- `MidPoint: Point3` - Midpoint coordinates
- `Direction: Point3` - Direction vector

**Example:**
```csharp
// Create from GeometryNodes
var startNode = new GeometryNode(0, 0, 0);
var endNode = new GeometryNode(5, 0, 0);
var line1 = new GeometryLine(startNode, endNode);

// Create directly from Point3
var line2 = new GeometryLine(
    new Point3(0, 0, 0),
    new Point3(5, 0, 0)
);

// Set material and cross section
var material = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var crossSection = new CrossSection_Box(0, "Box100", 100, 100, 5);

line2.Material = material;
line2.CrossSection = crossSection;
line2.BetaAngle = 45.0;

// Access line properties
Console.WriteLine($"Length: {line2.Length}");
Console.WriteLine($"MidPoint: {line2.MidPoint}");
```

### 3. ModelGeometryBuilder

Manages the conversion from GeometryLines to FEMur structural analysis models.

**Key Features:**
- Automatic node deduplication (with tolerance)
- Automatic ID assignment
- Application of default material/cross section

**Properties:**
- `NodeTolerance: double` - Tolerance for node deduplication (default: 1e-6)

**Methods:**
- `AddLine(GeometryLine)` - Add a line
- `AddLines(IEnumerable<GeometryLine>)` - Add multiple lines
- `GetNodes(): List<Node>` - Get merged Node list
- `BuildBeamElements(...): List<BeamElement>` - Generate BeamElement list
- `Clear()` - Clear builder state
- `GetStatistics(): string` - Get statistics

**Example:**
```csharp
using FEMur.Geometry.Intermediate;
using FEMur.Materials;
using FEMur.CrossSections;

// Create builder
var builder = new ModelGeometryBuilder(nodeTolerance: 1e-6);

// Define material and cross section
var material = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var crossSection = new CrossSection_Box(0, "Box100", 100, 100, 5);

// Add multiple lines (connection points are automatically merged)
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

// Generate nodes and elements
var nodes = builder.GetNodes();          // 4 unique nodes
var elements = builder.BuildBeamElements();  // 3 elements

Console.WriteLine($"Nodes: {nodes.Count}");       // 4
Console.WriteLine($"Elements: {elements.Count}"); // 3
Console.WriteLine(builder.GetStatistics());
```

### 4. GeometryConverter

Utility for converting between FEMur geometry types and intermediate representation types.

**Methods:**
- `ToGeometryNode(Point3): GeometryNode`
- `ToGeometryNodes(IEnumerable<Point3>): List<GeometryNode>`
- `ToGeometryLine(Point3, Point3): GeometryLine`
- `ToGeometryLines(IEnumerable<(Point3, Point3)>): List<GeometryLine>`
- `ToPoint3(GeometryNode): Point3`
- `ToPoint3List(IEnumerable<GeometryNode>): List<Point3>`

**Example:**
```csharp
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;

// Point3 → GeometryNode
var point = new Point3(1, 2, 3);
var node = GeometryConverter.ToGeometryNode(point);

// Multiple Point3 → GeometryNode list
var points = new List<Point3>
{
    new Point3(0, 0, 0),
    new Point3(1, 0, 0),
    new Point3(2, 0, 0)
};
var nodes = GeometryConverter.ToGeometryNodes(points);

// Line pairs → GeometryLines
var linePairs = new List<(Point3, Point3)>
{
    (new Point3(0, 0, 0), new Point3(1, 0, 0)),
    (new Point3(1, 0, 0), new Point3(2, 0, 0))
};
var lines = GeometryConverter.ToGeometryLines(linePairs);
```

## Practical Examples

### Example 1: Simple Beam Model

```csharp
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;
using FEMur.Materials;
using FEMur.CrossSections;
using FEMur.Models;

// Define material and cross section
var steel = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var section = new CrossSection_Box(0, "Box100", 100, 100, 5);

// Create builder
var builder = new ModelGeometryBuilder();

// 5m simple beam
var beam = new GeometryLine(new Point3(0, 0, 0), new Point3(5000, 0, 0))
{
    Material = steel,
    CrossSection = section
};

builder.AddLine(beam);

// Generate model
var nodes = builder.GetNodes();
var elements = builder.BuildBeamElements();

var model = new Model(nodes, elements.Cast<ElementBase>().ToList(), 
                      new List<Support>(), new List<Load>());
```

### Example 2: Truss Structure

```csharp
// Set default material and cross section
var steel = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
var section = new CrossSection_Box(0, "Box100", 100, 100, 5);

var builder = new ModelGeometryBuilder();

// Truss node coordinates
var node1 = new Point3(0, 0, 0);
var node2 = new Point3(5000, 0, 0);
var node3 = new Point3(10000, 0, 0);
var node4 = new Point3(2500, 3000, 0);
var node5 = new Point3(7500, 3000, 0);

// Truss members (material and cross section set later in batch)
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

// Generate elements with default material and cross section
var nodes = builder.GetNodes();
var elements = builder.BuildBeamElements(
    defaultMaterial: steel,
    defaultCrossSection: section
);

// Display statistics
Console.WriteLine(builder.GetStatistics());
// Output: GeometryNodes: 0, GeometryLines: 7, Nodes: 5, Elements: 7
```

## Benefits

### 1. Automated Node ID Management

Before:
```csharp
// Manual ID management required
var nodes = new List<Node>();
for (int i = 0; i < points.Count; i++)
{
    nodes.Add(new Node(i, points[i].X, points[i].Y, points[i].Z));
}
```

After:
```csharp
// IDs are automatically assigned
var builder = new ModelGeometryBuilder();
builder.AddLines(geometryLines);
var nodes = builder.GetNodes();  // IDs are automatically 0, 1, 2, ...
```

### 2. Automatic Duplicate Node Elimination

Before: Manual node duplication check required

After:
```csharp
// Automatically eliminates duplicates within tolerance
var builder = new ModelGeometryBuilder(nodeTolerance: 1e-6);
// Shared nodes are created only once even when connecting lines
```

### 3. Flexible Material/Cross Section Setting

```csharp
// Set individually for each line
line1.Material = steelMaterial;
line1.CrossSection = box100Section;

// Or set defaults in batch
var elements = builder.BuildBeamElements(
    defaultMaterial: steelMaterial,
    defaultCrossSection: box100Section
);
```

### 4. Extensibility

- `Tag` property allows storing arbitrary reference data
- Separation of geometry and structural analysis enables independent extension of each layer

## Testing

Comprehensive unit tests implemented:

- `GeometryNodeTest.cs` - GeometryNode functionality
- `GeometryLineTest.cs` - GeometryLine functionality
- `ModelGeometryBuilderTest.cs` - Builder functionality
- `GeometryConverterTest.cs` - Conversion utility functionality

All tests pass successfully.

## Summary

The introduction of intermediate classes achieves:

✅ Conversion from geometry to FEMur model without Node ID requirements  
✅ Automatic node deduplication and ID assignment  
✅ Flexible material and cross section configuration  
✅ Highly extensible design  
✅ Quality assurance through comprehensive testing

This significantly improves the maintainability of LineToBeam processing and other conversion operations.
