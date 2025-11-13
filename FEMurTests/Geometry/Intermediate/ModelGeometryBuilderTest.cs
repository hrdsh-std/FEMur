using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;
using FEMur.Geometry.Intermediate;
using FEMur.Materials;
using FEMur.CrossSections;
using System;
using System.Linq;

namespace FEMurTests.Geometry.Intermediate
{
    [TestClass]
    public class ModelGeometryBuilderTest
    {
        private Material_Isotropic _testMaterial;
        private CrossSection_Box _testCrossSection;

        [TestInitialize]
        public void Setup()
        {
            _testMaterial = new Material_Isotropic(0, "Steel", "S355", 200000, 0.3, 7850);
            _testCrossSection = new CrossSection_Box(0, "Box100", 100, 100, 5);
        }

        [TestMethod]
        public void AddLine_AddsLineToBuilder()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0));

            // Act
            builder.AddLine(line);
            var stats = builder.GetStatistics();

            // Assert
            Assert.IsTrue(stats.Contains("GeometryLines: 1"));
        }

        [TestMethod]
        public void BuildNodes_CreatesUniqueNodes()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line1 = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0));
            var line2 = new GeometryLine(new Point3(1, 0, 0), new Point3(2, 0, 0));

            // Act
            builder.AddLine(line1);
            builder.AddLine(line2);
            var nodes = builder.GetNodes();

            // Assert
            Assert.AreEqual(3, nodes.Count); // 3 unique nodes: (0,0,0), (1,0,0), (2,0,0)
            Assert.AreEqual(0, nodes[0].Id);
            Assert.AreEqual(1, nodes[1].Id);
            Assert.AreEqual(2, nodes[2].Id);
        }

        [TestMethod]
        public void BuildNodes_MergesDuplicateNodes()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var sharedPoint = new Point3(1, 0, 0);
            var line1 = new GeometryLine(new Point3(0, 0, 0), sharedPoint);
            var line2 = new GeometryLine(sharedPoint, new Point3(2, 0, 0));
            var line3 = new GeometryLine(new Point3(2, 0, 0), new Point3(3, 0, 0));

            // Act
            builder.AddLine(line1);
            builder.AddLine(line2);
            builder.AddLine(line3);
            var nodes = builder.GetNodes();

            // Assert
            Assert.AreEqual(4, nodes.Count); // 4 unique nodes
        }

        [TestMethod]
        public void BuildNodes_WithTolerance_MergesCloseNodes()
        {
            // Arrange
            var builder = new ModelGeometryBuilder(nodeTolerance: 0.01);
            var line1 = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0));
            var line2 = new GeometryLine(new Point3(1.005, 0, 0), new Point3(2, 0, 0)); // Close to (1,0,0)

            // Act
            builder.AddLine(line1);
            builder.AddLine(line2);
            var nodes = builder.GetNodes();

            // Assert
            Assert.AreEqual(3, nodes.Count); // Nodes at ~1.0 should be merged
        }

        [TestMethod]
        public void BuildBeamElements_CreatesElements()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                Material = _testMaterial,
                CrossSection = _testCrossSection
            };

            // Act
            builder.AddLine(line);
            var elements = builder.BuildBeamElements();

            // Assert
            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual(0, elements[0].Id);
            Assert.AreEqual(0, elements[0].NodeIds[0]);
            Assert.AreEqual(1, elements[0].NodeIds[1]);
        }

        [TestMethod]
        public void BuildBeamElements_UsesDefaultMaterial()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                CrossSection = _testCrossSection
            };

            // Act
            builder.AddLine(line);
            var elements = builder.BuildBeamElements(defaultMaterial: _testMaterial);

            // Assert
            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual(_testMaterial, elements[0].Material);
        }

        [TestMethod]
        public void BuildBeamElements_UsesDefaultCrossSection()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                Material = _testMaterial
            };

            // Act
            builder.AddLine(line);
            var elements = builder.BuildBeamElements(defaultMaterial: _testMaterial, defaultCrossSection: _testCrossSection);

            // Assert
            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual(_testCrossSection, elements[0].CrossSection);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BuildBeamElements_NoMaterial_ThrowsException()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0));

            // Act
            builder.AddLine(line);
            builder.BuildBeamElements();

            // Assert - Exception expected
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BuildBeamElements_NoCrossSection_ThrowsException()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                Material = _testMaterial
            };

            // Act
            builder.AddLine(line);
            builder.BuildBeamElements();

            // Assert - Exception expected
        }

        [TestMethod]
        public void BuildBeamElements_WithBetaAngle_SetsBetaAngle()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                Material = _testMaterial,
                CrossSection = _testCrossSection,
                BetaAngle = 45.0
            };

            // Act
            builder.AddLine(line);
            var elements = builder.BuildBeamElements();

            // Assert
            Assert.AreEqual(45.0, elements[0].BetaAngle);
        }

        [TestMethod]
        public void BuildBeamElements_MultipleLines_CreatesMultipleElements()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line1 = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                Material = _testMaterial,
                CrossSection = _testCrossSection
            };
            var line2 = new GeometryLine(new Point3(1, 0, 0), new Point3(2, 0, 0))
            {
                Material = _testMaterial,
                CrossSection = _testCrossSection
            };

            // Act
            builder.AddLine(line1);
            builder.AddLine(line2);
            var elements = builder.BuildBeamElements();
            var nodes = builder.GetNodes();

            // Assert
            Assert.AreEqual(2, elements.Count);
            Assert.AreEqual(3, nodes.Count); // 3 unique nodes
            
            // Check element connectivity
            Assert.AreEqual(0, elements[0].NodeIds[0]);
            Assert.AreEqual(1, elements[0].NodeIds[1]);
            Assert.AreEqual(1, elements[1].NodeIds[0]);
            Assert.AreEqual(2, elements[1].NodeIds[1]);
        }

        [TestMethod]
        public void Clear_RemovesAllData()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0));
            builder.AddLine(line);

            // Act
            builder.Clear();
            var stats = builder.GetStatistics();

            // Assert
            Assert.IsTrue(stats.Contains("GeometryLines: 0"));
            Assert.IsTrue(stats.Contains("Nodes: 0"));
        }

        [TestMethod]
        public void GetStatistics_ReturnsCorrectInformation()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var line1 = new GeometryLine(new Point3(0, 0, 0), new Point3(1, 0, 0))
            {
                Material = _testMaterial,
                CrossSection = _testCrossSection
            };
            var line2 = new GeometryLine(new Point3(1, 0, 0), new Point3(2, 0, 0))
            {
                Material = _testMaterial,
                CrossSection = _testCrossSection
            };

            // Act
            builder.AddLine(line1);
            builder.AddLine(line2);
            builder.BuildBeamElements();
            var stats = builder.GetStatistics();

            // Assert
            Assert.IsTrue(stats.Contains("GeometryLines: 2"));
            Assert.IsTrue(stats.Contains("Nodes: 3"));
            Assert.IsTrue(stats.Contains("Elements: 2"));
        }

        [TestMethod]
        public void AddNode_ExplicitlyAddsNode()
        {
            // Arrange
            var builder = new ModelGeometryBuilder();
            var node = new GeometryNode(5, 5, 5);

            // Act
            builder.AddNode(node);
            var nodes = builder.GetNodes();

            // Assert
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(5.0, nodes[0].Position.X);
        }
    }
}
