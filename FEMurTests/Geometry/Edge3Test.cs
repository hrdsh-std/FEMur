using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;

namespace FEMurTests
{
    [TestClass]
    public class Edge3Test
    {
        [TestMethod]
        //�C���f�b�N�X���g�p���Ē��_�ɃA�N�Z�X�ł��邱�Ƃ��m�F����
        public void IndexAccessTest()
        {
            Edge3 edge = new Edge3(1, 2);
            Assert.AreEqual(edge.A, edge[0]);
            Assert.AreEqual(edge.B, edge[1]);
        }

        //���_�̏������t�ł������ł��邱�Ƃ��m�F����
        [TestMethod]
        public void EqualityTest()
        {
            Edge3 edge = new Edge3(1, 2);
            Edge3 edgeReversed = edge.Opposite();
            Edge3 expected = new Edge3(2, 1);

            Assert.AreEqual(expected, edgeReversed);
        }
        //Normalized���\�b�h�����������삷�邱�Ƃ��m�F����
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
        //operator�̃e�X�g
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
