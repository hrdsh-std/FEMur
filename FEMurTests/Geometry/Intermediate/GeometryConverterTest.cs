using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;
using System.Collections.Generic;
using System.Linq;

namespace FEMurTests.Geometry.Intermediate
{
    [TestClass]
    public class GeometryConverterTest
    {
        [TestMethod]
        public void ToGeometryNode_ConvertsPoint3()
        {
            // Arrange
            var point = new Point3(1, 2, 3);

            // Act
            var node = GeometryConverter.ToGeometryNode(point);

            // Assert
            Assert.AreEqual(1.0, node.Position.X);
            Assert.AreEqual(2.0, node.Position.Y);
            Assert.AreEqual(3.0, node.Position.Z);
        }

        [TestMethod]
        public void ToGeometryNodes_ConvertsMultiplePoints()
        {
            // Arrange
            var points = new List<Point3>
            {
                new Point3(1, 2, 3),
                new Point3(4, 5, 6),
                new Point3(7, 8, 9)
            };

            // Act
            var nodes = GeometryConverter.ToGeometryNodes(points);

            // Assert
            Assert.AreEqual(3, nodes.Count);
            Assert.AreEqual(1.0, nodes[0].Position.X);
            Assert.AreEqual(4.0, nodes[1].Position.X);
            Assert.AreEqual(7.0, nodes[2].Position.X);
        }

        [TestMethod]
        public void ToGeometryLine_CreatesLineFromPoints()
        {
            // Arrange
            var start = new Point3(0, 0, 0);
            var end = new Point3(1, 0, 0);

            // Act
            var line = GeometryConverter.ToGeometryLine(start, end);

            // Assert
            Assert.AreEqual(0.0, line.StartNode.Position.X);
            Assert.AreEqual(1.0, line.EndNode.Position.X);
            Assert.AreEqual(1.0, line.Length);
        }

        [TestMethod]
        public void ToGeometryLines_CreatesMultipleLines()
        {
            // Arrange
            var linePairs = new List<(Point3, Point3)>
            {
                (new Point3(0, 0, 0), new Point3(1, 0, 0)),
                (new Point3(1, 0, 0), new Point3(2, 0, 0)),
                (new Point3(2, 0, 0), new Point3(3, 0, 0))
            };

            // Act
            var lines = GeometryConverter.ToGeometryLines(linePairs);

            // Assert
            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(1.0, lines[0].Length);
            Assert.AreEqual(1.0, lines[1].Length);
            Assert.AreEqual(1.0, lines[2].Length);
        }

        [TestMethod]
        public void ToPoint3_ConvertsGeometryNode()
        {
            // Arrange
            var node = new GeometryNode(1, 2, 3);

            // Act
            var point = GeometryConverter.ToPoint3(node);

            // Assert
            Assert.AreEqual(1.0, point.X);
            Assert.AreEqual(2.0, point.Y);
            Assert.AreEqual(3.0, point.Z);
        }

        [TestMethod]
        public void ToPoint3List_ConvertsMultipleNodes()
        {
            // Arrange
            var nodes = new List<GeometryNode>
            {
                new GeometryNode(1, 2, 3),
                new GeometryNode(4, 5, 6),
                new GeometryNode(7, 8, 9)
            };

            // Act
            var points = GeometryConverter.ToPoint3List(nodes);

            // Assert
            Assert.AreEqual(3, points.Count);
            Assert.AreEqual(1.0, points[0].X);
            Assert.AreEqual(4.0, points[1].X);
            Assert.AreEqual(7.0, points[2].X);
        }

        [TestMethod]
        public void RoundTripConversion_MaintainsData()
        {
            // Arrange
            var originalPoint = new Point3(1.5, 2.5, 3.5);

            // Act
            var node = GeometryConverter.ToGeometryNode(originalPoint);
            var convertedPoint = GeometryConverter.ToPoint3(node);

            // Assert
            Assert.AreEqual(originalPoint.X, convertedPoint.X);
            Assert.AreEqual(originalPoint.Y, convertedPoint.Y);
            Assert.AreEqual(originalPoint.Z, convertedPoint.Z);
        }
    }
}
