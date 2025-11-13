using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;
using System;

namespace FEMurTests.Geometry.Intermediate
{
    [TestClass]
    public class GeometryNodeTest
    {
        [TestMethod]
        public void Constructor_FromPoint3_CreatesNode()
        {
            // Arrange
            var point = new Point3(1.0, 2.0, 3.0);

            // Act
            var node = new GeometryNode(point);

            // Assert
            Assert.AreEqual(1.0, node.Position.X);
            Assert.AreEqual(2.0, node.Position.Y);
            Assert.AreEqual(3.0, node.Position.Z);
        }

        [TestMethod]
        public void Constructor_FromCoordinates_CreatesNode()
        {
            // Arrange & Act
            var node = new GeometryNode(1.0, 2.0, 3.0);

            // Assert
            Assert.AreEqual(1.0, node.Position.X);
            Assert.AreEqual(2.0, node.Position.Y);
            Assert.AreEqual(3.0, node.Position.Z);
        }

        [TestMethod]
        public void ToNode_ConvertsToFEMNode()
        {
            // Arrange
            var geometryNode = new GeometryNode(1.0, 2.0, 3.0);

            // Act
            var femNode = geometryNode.ToNode(5);

            // Assert
            Assert.AreEqual(5, femNode.Id);
            Assert.AreEqual(1.0, femNode.Position.X);
            Assert.AreEqual(2.0, femNode.Position.Y);
            Assert.AreEqual(3.0, femNode.Position.Z);
        }

        [TestMethod]
        public void Equals_SamePosition_ReturnsTrue()
        {
            // Arrange
            var node1 = new GeometryNode(1.0, 2.0, 3.0);
            var node2 = new GeometryNode(1.0, 2.0, 3.0);

            // Act & Assert
            Assert.IsTrue(node1.Equals(node2));
            Assert.AreEqual(node1.GetHashCode(), node2.GetHashCode());
        }

        [TestMethod]
        public void Equals_DifferentPosition_ReturnsFalse()
        {
            // Arrange
            var node1 = new GeometryNode(1.0, 2.0, 3.0);
            var node2 = new GeometryNode(1.0, 2.0, 4.0);

            // Act & Assert
            Assert.IsFalse(node1.Equals(node2));
        }

        [TestMethod]
        public void Tag_CanStoreReferenceData()
        {
            // Arrange
            var node = new GeometryNode(1.0, 2.0, 3.0);
            var tag = new { Name = "TestNode", Id = 42 };

            // Act
            node.Tag = tag;

            // Assert
            Assert.IsNotNull(node.Tag);
            Assert.AreEqual(tag, node.Tag);
        }

        [TestMethod]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var node = new GeometryNode(1.234, 2.345, 3.456);

            // Act
            var result = node.ToString();

            // Assert
            Assert.IsTrue(result.Contains("GeometryNode"));
            Assert.IsTrue(result.Contains("1.234"));
            Assert.IsTrue(result.Contains("2.345"));
            Assert.IsTrue(result.Contains("3.456"));
        }
    }
}
