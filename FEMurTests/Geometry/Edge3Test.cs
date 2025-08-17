using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;

namespace FEMurTests
{
    [TestClass]
    public class Edge3Test
    {
        [TestMethod]
        //インデックスを使用して頂点にアクセスできることを確認する
        public void IndexAccessTest()
        {
            Edge3 edge = new Edge3(1, 2);
            Assert.AreEqual(edge.A, edge[0]);
            Assert.AreEqual(edge.B, edge[1]);
        }

        //頂点の順序が逆でも等価であることを確認する
        [TestMethod]
        public void EqualityTest()
        {
            Edge3 edge = new Edge3(1, 2);
            Edge3 edgeReversed = edge.Opposite();
            Edge3 expected = new Edge3(2, 1);

            Assert.AreEqual(expected, edgeReversed);
        }
        //Normalizedメソッドが正しく動作することを確認する
        [TestMethod]
        public void NormalizedTest()
        {
            Edge3 edge1 = new Edge3(2, 1);
            Edge3 normalized1 = edge1.Normalized();
            Edge3 edge2 = new Edge3(1, 2);
            Edge3 normalized2 = edge2.Normalized();

            Edge3 expected = new Edge3(1, 2);
            Assert.AreEqual(expected, normalized1);
            Assert.AreEqual(expected, normalized2);
        }
        //operatorのテスト
        [TestMethod]
        public void OperatorTest()
        {
            Edge3 edge1 = new Edge3(1, 2);
            Edge3 edge2 = new Edge3(2, 1);
            Edge3 edge3 = new Edge3(1, 3);
            Assert.IsTrue(edge1 == edge2);
            Assert.IsFalse(edge1 != edge2);
            Assert.IsFalse(edge1 == edge3);
            Assert.IsTrue(edge1 != edge3);
        }
    }
}
