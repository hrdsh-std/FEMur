using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;
using System;

namespace FEMurTests.Geometry.Intermediate
{
    [TestClass]
    public class GeometryLineTest
    {
        [TestMethod]
        public void Constructor_FromNodes_CreatesLine()
        {
            // Arrange
            var startNode = new GeometryNode(0, 0, 0);
            var endNode = new GeometryNode(1, 0, 0);

            // Act
            var line = new GeometryLine(startNode, endNode);

            // Assert
            Assert.AreEqual(startNode, line.StartNode);
            Assert.AreEqual(endNode, line.EndNode);
        }

        [TestMethod]
        public void Constructor_FromPoints_CreatesLine()
        {
            // Arrange
            var start = new Point3(0, 0, 0);
            var end = new Point3(1, 0, 0);

            // Act
            var line = new GeometryLine(start, end);

            // Assert
            Assert.AreEqual(0, line.StartNode.Position.X);
            Assert.AreEqual(1, line.EndNode.Position.X);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullStartNode_ThrowsException()
        {
            // Arrange & Act & Assert
            var line = new GeometryLine(null, new GeometryNode(1, 0, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullEndNode_ThrowsException()
        {
            // Arrange & Act & Assert
            var line = new GeometryLine(new GeometryNode(0, 0, 0), null);
        }

        [TestMethod]
        public void Length_CalculatesCorrectly()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(3, 4, 0)
            );

            // Act
            var length = line.Length;

            // Assert
            Assert.AreEqual(5.0, length, 1e-10);
        }

        [TestMethod]
        public void Length_3DLine_CalculatesCorrectly()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(1, 1, 1)
            );

            // Act
            var length = line.Length;

            // Assert
            Assert.AreEqual(Math.Sqrt(3), length, 1e-10);
        }

        [TestMethod]
        public void MidPoint_CalculatesCorrectly()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(2, 4, 6)
            );

            // Act
            var midPoint = line.MidPoint;

            // Assert
            Assert.AreEqual(1.0, midPoint.X);
            Assert.AreEqual(2.0, midPoint.Y);
            Assert.AreEqual(3.0, midPoint.Z);
        }

        [TestMethod]
        public void Direction_CalculatesCorrectly()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(1, 2, 3),
                new Point3(4, 6, 9)
            );

            // Act
            var direction = line.Direction;

            // Assert
            Assert.AreEqual(3.0, direction.X);
            Assert.AreEqual(4.0, direction.Y);
            Assert.AreEqual(6.0, direction.Z);
        }

        [TestMethod]
        public void BetaAngle_DefaultValue_IsZero()
        {
            // Arrange & Act
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(1, 0, 0)
            );

            // Assert
            Assert.AreEqual(0.0, line.BetaAngle);
        }

        [TestMethod]
        public void BetaAngle_CanBeSet()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(1, 0, 0)
            );

            // Act
            line.BetaAngle = 45.0;

            // Assert
            Assert.AreEqual(45.0, line.BetaAngle);
        }

        [TestMethod]
        public void MaterialAndCrossSection_CanBeSet()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(1, 0, 0)
            );
            var material = new FEMur.Materials.Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
            var crossSection = new FEMur.CrossSections.CrossSection_Box(0, "Box100x100x5", 100, 100, 5);

            // Act
            line.Material = material;
            line.CrossSection = crossSection;

            // Assert
            Assert.AreEqual(material, line.Material);
            Assert.AreEqual(crossSection, line.CrossSection);
        }

        [TestMethod]
        public void Tag_CanStoreReferenceData()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(1, 0, 0)
            );
            var tag = Guid.NewGuid();

            // Act
            line.Tag = tag;

            // Assert
            Assert.AreEqual(tag, line.Tag);
        }

        [TestMethod]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var line = new GeometryLine(
                new Point3(0, 0, 0),
                new Point3(1, 0, 0)
            );

            // Act
            var result = line.ToString();

            // Assert
            Assert.IsTrue(result.Contains("GeometryLine"));
            Assert.IsTrue(result.Contains("Start"));
            Assert.IsTrue(result.Contains("End"));
            Assert.IsTrue(result.Contains("Length"));
        }
    }
}
