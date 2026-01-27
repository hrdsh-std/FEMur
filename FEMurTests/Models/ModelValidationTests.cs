using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using FEMur.Models;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.CrossSections;
using FEMur.Supports;
using FEMur.Loads;
using FEMur.Geometry;

namespace FEMurTests.Models
{
    [TestClass]
    public class ModelValidationTests
    {
        #region Test Setup Helpers

        private Material CreateTestMaterial()
        {
            return Material_Isotropic.Steel();
        }

        private CrossSection_Beam CreateTestCrossSection()
        {
            return new CrossSection_Box("TestBox", 100, 100, 5);
        }

        #endregion

        #region 正常系テスト

        [TestMethod]
        public void ValidateAndRepairModel_ValidModel_NoErrors()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0,new Point3(0.0,0.0,0.0)),
                new Node(1,new Point3(1.0,0.0,0.0))
            };

            var elements = new List<ElementBase>
            {
                new BeamElement(0, nodes[0], nodes[1], CreateTestMaterial(), CreateTestCrossSection())
            };

            // Act
            var model = new Model(nodes, elements, new List<Support>(), new List<Load>());

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Nodes.Count);
            Assert.AreEqual(1, model.Elements.Count);
        }

        [TestMethod]
        public void ValidateAndRepairModel_EmptyModel_NoErrors()
        {
            // Arrange & Act
            var model = new Model(
                new List<Node>(),
                new List<ElementBase>(),
                new List<Support>(),
                new List<Load>());

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Nodes.Count);
            Assert.AreEqual(0, model.Elements.Count);
        }

        #endregion

        #region 自動ノード生成テスト

        [TestMethod]
        public void ValidateAndRepairModel_ElementWithPoints_AutoGeneratesNodes()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1, 0, 0);

            var element = new BeamElement(0, point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Act
            var model = new Model(new List<Node>(), elements, new List<Support>(), new List<Load>());

            // Assert
            Assert.AreEqual(2, model.Nodes.Count);
            Assert.IsNotNull(element.NodeIds);
            Assert.AreEqual(2, element.NodeIds.Count);

            // 生成されたノードの座標を確認
            var node0 = model.Nodes[0];
            var node1 = model.Nodes[1];
            Assert.AreEqual(point1.X, node0.Position.X);
            Assert.AreEqual(point1.Y, node0.Position.Y);
            Assert.AreEqual(point1.Z, node0.Position.Z);
            Assert.AreEqual(point2.X, node1.Position.X);
            Assert.AreEqual(point2.Y, node1.Position.Y);
            Assert.AreEqual(point2.Z, node1.Position.Z);
        }

        [TestMethod]
        public void ValidateAndRepairModel_ElementWithPoints_ReusesExistingNodes()
        {
            // Arrange
            var existingNode = new Node(0, 0, 0, 0);
            var nodes = new List<Node> { existingNode };

            var point1 = new Point3(0, 0, 0); // 既存ノードと同じ座標
            var point2 = new Point3(1, 0, 0);
            var points = new List<Point3> { point1, point2 };

            var element = new BeamElement(0, point1,point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Act
            var model = new Model(nodes, elements, new List<Support>(), new List<Load>());

            // Assert
            Assert.AreEqual(2, model.Nodes.Count); // 既存1 + 新規1
            Assert.IsNotNull(element.NodeIds);
            Assert.AreEqual(2, element.NodeIds.Count);
            Assert.AreEqual(0, element.NodeIds[0]); // 既存ノードのIDを使用
        }

        [TestMethod]
        public void ValidateAndRepairModel_MultipleElementsWithSamePoints_SharesNodes()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1, 0, 0);
            var point3 = new Point3(2, 0, 0);

            var element1 = new BeamElement(0, point1, point2,
                CreateTestMaterial(), CreateTestCrossSection());
            var element2 = new BeamElement(1,point2, point3,
                CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element1, element2 };

            // Act
            var model = new Model(new List<Node>(), elements, new List<Support>(), new List<Load>());

            // Assert
            Assert.AreEqual(3, model.Nodes.Count); // point1, point2, point3
            Assert.AreEqual(element1.NodeIds[1], element2.NodeIds[0]); // point2を共有
        }

        [TestMethod]
        public void ValidateAndRepairModel_ElementWithPointsWithinTolerance_ReusesNode()
        {
            // Arrange
            var existingNode = new Node(0, 0, 0, 0);
            var nodes = new List<Node> { existingNode };

            var point1 = new Point3(0.0005, 0.0005, 0.0005); // 許容誤差(0.001)以内
            var point2 = new Point3(1, 0, 0);

            var element = new BeamElement(0, point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Act
            var model = new Model(nodes, elements, new List<Support>(), new List<Load>());

            // Assert
            Assert.AreEqual(2, model.Nodes.Count); // 既存ノードを再利用
            Assert.AreEqual(0, element.NodeIds[0]); // 既存ノードのIDを使用
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndRepairModel_ElementReferencingNonExistentNode_ThrowsException()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0)
            };

            var element = new BeamElement(0, 0, 999, CreateTestMaterial(), CreateTestCrossSection()); // ノードID 999は存在しない
            var elements = new List<ElementBase> { element };

            // Act
            var model = new Model(nodes, elements, new List<Support>(), new List<Load>());
        }

        [TestMethod]
        public void ValidateAndRepairModel_ElementReferencingNonExistentNode_ContainsErrorMessage()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0)
            };

            var element = new BeamElement(0, 0, 999, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Act & Assert
            try
            {
                var model = new Model(nodes, elements, new List<Support>(), new List<Load>());
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "References non-existent Node ID 999");
            }
        }

        #region エラー検出テスト - 支持条件

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndRepairModel_SupportReferencingNonExistentNode_ThrowsException()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0)
            };

            var support = new Support(999,true, true, true, false, false, false); // ノードID 999は存在しない
            var supports = new List<Support> { support };

            // Act
            var model = new Model(nodes, new List<ElementBase>(), supports, new List<Load>());
        }

        [TestMethod]
        public void ValidateAndRepairModel_SupportReferencingNonExistentNode_ContainsErrorMessage()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0)
            };

            var support = new Support( 999, true, true, true, false, false, false );
            var supports = new List<Support> { support };

            // Act & Assert
            try
            {
                var model = new Model(nodes, new List<ElementBase>(), supports, new List<Load>());
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "Support references non-existent Node ID 999");
            }
        }

        #endregion

        #region エラー検出テスト - 荷重

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndRepairModel_PointLoadReferencingNonExistentNode_ThrowsException()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0)
            };

            var load = new PointLoad(999, new Vector3(0, 0, -100), new Vector3(0, 0, 0)); // ノードID 999は存在しない
            var loads = new List<Load> { load };

            // Act
            var model = new Model(nodes, new List<ElementBase>(), new List<Support>(), loads);
        }

        [TestMethod]
        public void ValidateAndRepairModel_PointLoadReferencingNonExistentNode_ContainsErrorMessage()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0)
            };

            var load = new PointLoad(999, new Vector3(0, 0, -100), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Act & Assert
            try
            {
                var model = new Model(nodes, new List<ElementBase>(), new List<Support>(), loads);
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "PointLoad references non-existent Node ID 999");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndRepairModel_ElementLoadReferencingNonExistentElement_ThrowsException()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0),
                new Node(1, 1, 0, 0)
            };

            var element = new BeamElement(0, nodes[0], nodes[1], CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            var load = new ElementLoad(999, new Vector3(0, 0, -10)); // 要素ID 999は存在しない
            var loads = new List<Load> { load };

            // Act
            var model = new Model(nodes, elements, new List<Support>(), loads);
        }

        [TestMethod]
        public void ValidateAndRepairModel_ElementLoadReferencingNonExistentElement_ContainsErrorMessage()
        {
            // Arrange
            var nodes = new List<Node>
            {
                new Node(0, 0, 0, 0),
                new Node(1, 1, 0, 0)
            };

            var element = new BeamElement(0, nodes[0], nodes[1], CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            var load = new ElementLoad(999, new Vector3(0, 0, -10));
            var loads = new List<Load> { load };

            // Act & Assert
            try
            {
                var model = new Model(nodes, elements, new List<Support>(), loads);
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "ElementLoad references non-existent Element ID 999");
            }
        }

        #endregion

        #region 複合テスト

        [TestMethod]
        public void ValidateAndRepairModel_ComplexModelWithMixedNodesAndPoints_ValidatesCorrectly()
        {
            // Arrange
            var existingNodes = new List<Node>
            {
                new Node(0, 0, 0, 0),
                new Node(1, 1, 0, 0)
            };

            var element1 = new BeamElement(0, existingNodes[0], existingNodes[1],
                CreateTestMaterial(), CreateTestCrossSection());

            var element2 = new BeamElement(1,
                new Point3(1, 0, 0), new Point3(2, 0, 0),
                CreateTestMaterial(), CreateTestCrossSection());

            var elements = new List<ElementBase> { element1, element2 };

            var support = new Support( 0, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            var load = new PointLoad(1, new Vector3(0, 0, -100), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Act
            var model = new Model(existingNodes, elements, supports, loads);

            // Assert
            Assert.AreEqual(3, model.Nodes.Count); // 既存2 + 新規1
            Assert.AreEqual(2, model.Elements.Count);
            Assert.IsNotNull(element2.NodeIds);
            Assert.AreEqual(2, element2.NodeIds.Count);
            Assert.AreEqual(1, element2.NodeIds[0]); // 既存ノードを再利用
        }

        #endregion

        #region ノードID自動採番テスト

        [TestMethod]
        public void ValidateAndRepairModel_AutoGeneratedNodes_UseCorrectIds()
        {
            // Arrange
            var existingNodes = new List<Node>
            {
                new Node(0, 0, 0, 0),
                new Node(1, 1, 0, 0),
                new Node(5, 2, 0, 0) // ID 5（連続していない）
            };

            var element = new BeamElement(0,
                new Point3(3, 0, 0), new Point3(4, 0, 0),
                CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Act
            var model = new Model(existingNodes, elements, new List<Support>(), new List<Load>());

            // Assert
            Assert.AreEqual(5, model.Nodes.Count); // 既存3 + 新規2

            // 新規ノードのIDは最大ID(5) + 1 から開始
            var newNodeIds = element.NodeIds;
            Assert.IsTrue(newNodeIds[0] >= 6);
            Assert.IsTrue(newNodeIds[1] >= 6);
        }

        #endregion

        #region Point3指定のSupport/Loadテスト

        [TestMethod]
        public void ValidateAndRepairModel_SupportWithPoint3_FindsExistingNode()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            // 要素を作成（ノードが自動生成される）
            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Point3で支持条件を指定（要素の始点と同じ座標）
            var support = new Support(point1, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            // Act
            var model = new Model(new List<Node>(), elements, supports, new List<Load>());

            // Assert
            Assert.AreEqual(2, model.Nodes.Count); // 要素から2つのノードが生成
            Assert.IsTrue(support.NodeId >= 0, "SupportのNodeIDが設定されるべき");
            Assert.AreEqual(element.NodeIds[0], support.NodeId, "Supportは要素の始点ノードを参照すべき");
        }

        [TestMethod]
        public void ValidateAndRepairModel_PointLoadWithPoint3_FindsExistingNode()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            // 要素を作成（ノードが自動生成される）
            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Point3で荷重を指定（要素の終点と同じ座標）
            var load = new PointLoad(point2, new Vector3(0, -100, 0));
            var loads = new List<Load> { load };

            // Act
            var model = new Model(new List<Node>(), elements, new List<Support>(), loads);

            // Assert
            Assert.AreEqual(2, model.Nodes.Count); // 要素から2つのノードが生成
            Assert.IsTrue(load.NodeId >= 0, "LoadのNodeIDが設定されるべき");
            Assert.AreEqual(element.NodeIds[1], load.NodeId, "Loadは要素の終点ノードを参照すべき");
        }

        [TestMethod]
        public void ValidateAndRepairModel_SupportAndLoadWithPoint3_WithinTolerance()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            // 要素を作成
            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // 許容誤差(0.001)以内の座標で支持条件と荷重を指定
            var supportPoint = new Point3(0.0005, 0.0005, 0.0005);
            var loadPoint = new Point3(1000.0005, -0.0005, 0.0005);

            var support = new Support(supportPoint, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            var load = new PointLoad(loadPoint, new Vector3(0, -100, 0));
            var loads = new List<Load> { load };

            // Act
            var model = new Model(new List<Node>(), elements, supports, loads);

            // Assert
            Assert.AreEqual(2, model.Nodes.Count); // 要素から2つのノードが生成（許容誤差内で再利用）
            Assert.AreEqual(element.NodeIds[0], support.NodeId, "Supportは始点ノードを再利用すべき");
            Assert.AreEqual(element.NodeIds[1], load.NodeId, "Loadは終点ノードを再利用すべき");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndRepairModel_SupportWithPoint3_NoMatchingNode_ThrowsException()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            // 要素を作成
            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // 要素のノードと一致しない座標で支持条件を指定
            var supportPoint = new Point3(500, 0, 0); // 中間点（ノードが存在しない）
            var support = new Support(supportPoint, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            // Act - 例外がスローされることを期待
            var model = new Model(new List<Node>(), elements, supports, new List<Load>());
        }

        [TestMethod]
        public void ValidateAndRepairModel_SupportWithPoint3_NoMatchingNode_ContainsErrorMessage()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            var supportPoint = new Point3(500, 0, 0);
            var support = new Support(supportPoint, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            // Act & Assert
            try
            {
                var model = new Model(new List<Node>(), elements, supports, new List<Load>());
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "No existing node found at position");
                StringAssert.Contains(ex.Message, "500");
                StringAssert.Contains(ex.Message, "Support must reference an existing node");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndRepairModel_PointLoadWithPoint3_NoMatchingNode_ThrowsException()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // 要素のノードと一致しない座標で荷重を指定
            var loadPoint = new Point3(500, 0, 0);
            var load = new PointLoad(loadPoint, new Vector3(0, -100, 0));
            var loads = new List<Load> { load };

            // Act
            var model = new Model(new List<Node>(), elements, new List<Support>(), loads);
        }

        [TestMethod]
        public void ValidateAndRepairModel_PointLoadWithPoint3_NoMatchingNode_ContainsErrorMessage()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            var loadPoint = new Point3(500, 0, 0);
            var load = new PointLoad(loadPoint, new Vector3(0, -100, 0));
            var loads = new List<Load> { load };

            // Act & Assert
            try
            {
                var model = new Model(new List<Node>(), elements, new List<Support>(), loads);
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "No existing node found at position");
                StringAssert.Contains(ex.Message, "500");
                StringAssert.Contains(ex.Message, "must reference an existing node");
            }
        }

        [TestMethod]
        public void ValidateAndRepairModel_MultipleElementsWithPoint3SupportAndLoad()
        {
            // Arrange - 門型フレーム
            var bottomLeft = new Point3(0, 0, 0);
            var topLeft = new Point3(0, 3000, 0);
            var topRight = new Point3(4000, 3000, 0);
            var bottomRight = new Point3(4000, 0, 0);

            // 3つの要素を作成
            var element1 = new BeamElement(bottomLeft, topLeft, CreateTestMaterial(), CreateTestCrossSection()); // 左柱
            var element2 = new BeamElement(topLeft, topRight, CreateTestMaterial(), CreateTestCrossSection());   // 梁
            var element3 = new BeamElement(topRight, bottomRight, CreateTestMaterial(), CreateTestCrossSection()); // 右柱
            var elements = new List<ElementBase> { element1, element2, element3 };

            // Point3で支持条件を指定（両端固定）
            var support1 = new Support(bottomLeft, true, true, true, true, true, true);
            var support2 = new Support(bottomRight, true, true, true, true, true, true);
            var supports = new List<Support> { support1, support2 };

            // Point3で荷重を指定（左上に水平荷重）
            var load = new PointLoad(topLeft, new Vector3(100, 0, 0));
            var loads = new List<Load> { load };

            // Act
            var model = new Model(new List<Node>(), elements, supports, loads);

            // Assert
            Assert.AreEqual(4, model.Nodes.Count); // 4つのノードが生成
            Assert.AreEqual(3, model.Elements.Count);
            
            // 支持条件のノードIDが正しく設定されている
            Assert.IsTrue(support1.NodeId >= 0);
            Assert.IsTrue(support2.NodeId >= 0);
            
            // 荷重のノードIDが正しく設定されている
            Assert.IsTrue(load.NodeId >= 0);
            
            // 要素が正しくノードを共有している
            Assert.AreEqual(element1.NodeIds[1], element2.NodeIds[0], "左柱と梁は左上ノードを共有");
            Assert.AreEqual(element2.NodeIds[1], element3.NodeIds[0], "梁と右柱は右上ノードを共有");
            
            // 支持条件が正しいノードを参照している
            Assert.AreEqual(element1.NodeIds[0], support1.NodeId, "support1は左柱の始点を参照");
            Assert.AreEqual(element3.NodeIds[1], support2.NodeId, "support2は右柱の終点を参照");
            
            // 荷重が正しいノードを参照している
            Assert.AreEqual(element1.NodeIds[1], load.NodeId, "loadは左上ノードを参照");
        }

        [TestMethod]
        public void ValidateAndRepairModel_Point3SupportAndLoad_BeyondTolerance_ThrowsException()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // 許容誤差(0.001)を超える座標で支持条件を指定
            var supportPoint = new Point3(0.002, 0.002, 0.002); // 距離 = sqrt(3*0.002^2) ? 0.00346 > 0.001
            var support = new Support(supportPoint, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            // Act & Assert
            try
            {
                var model = new Model(new List<Node>(), elements, supports, new List<Load>());
                Assert.Fail("許容誤差を超える場合は例外が投げられるべき");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, "No existing node found");
            }
        }

        [TestMethod]
        public void ValidateAndRepairModel_MixedNodeIDAndPoint3Support()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);
            var point3 = new Point3(2000, 0, 0);

            var element1 = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var element2 = new BeamElement(point2, point3, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element1, element2 };

            // 最初のModelでノードIDを確定
            var tempModel = new Model(new List<Node>(), elements, new List<Support>(), new List<Load>());

            // NodeIDで支持条件を指定（始点）
            var support1 = new Support(element1.NodeIds[0], true, true, true, true, true, true);
            
            // Point3で支持条件を指定（終点）
            var support2 = new Support(point3, false, true, true, false, false, false);
            var supports = new List<Support> { support1, support2 };

            // 再度Model作成（既にNodeIdsが設定されているので再利用される）
            var model = new Model(tempModel.Nodes, elements, supports, new List<Load>());

            // Assert
            Assert.AreEqual(3, model.Nodes.Count);
            Assert.AreEqual(element1.NodeIds[0], support1.NodeId);
            Assert.AreEqual(element2.NodeIds[1], support2.NodeId);
        }

        [TestMethod]
        public void ValidateAndRepairModel_PointDisplacementWithPoint3()
        {
            // Arrange
            var point1 = new Point3(0, 0, 0);
            var point2 = new Point3(1000, 0, 0);

            var element = new BeamElement(point1, point2, CreateTestMaterial(), CreateTestCrossSection());
            var elements = new List<ElementBase> { element };

            // Point3で強制変位を指定
            var displacement = new PointDisplacement(point2, new Vector3(0, 10, 0));
            var loads = new List<Load> { displacement };

            // Act
            var model = new Model(new List<Node>(), elements, new List<Support>(), loads);

            // Assert
            Assert.AreEqual(2, model.Nodes.Count);
            Assert.IsTrue(displacement.NodeId >= 0);
            Assert.AreEqual(element.NodeIds[1], displacement.NodeId);
        }

        #endregion
    }
}
