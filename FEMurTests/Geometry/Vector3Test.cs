using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;

namespace FEMurTests
{
    [TestClass]
    public class Vector3Test
    {
        [TestMethod]
        // operatorのテスト
        public void OperatorTest()
        {
            var v1 = new Vector3(1.0, 2.0, 3.0);
            var v2 = new Vector3(4.0, 5.0, 6.0);
            var v3 = v1 + v2;
            Assert.AreEqual(new Vector3(5.0, 7.0, 9.0), v3);
            var v4 = v1 - v2;
            Assert.AreEqual(new Vector3(-3.0, -3.0, -3.0), v4);
            var dotProduct = Vector3.Dot(v1, v2);
            Assert.AreEqual(32.0, dotProduct);
            var crossProduct = Vector3.Cross(v1, v2);
            Assert.AreEqual(new Vector3(-3.0, 6.0, -3.0), crossProduct);
            var v5 = v1 * 2.0;
            Assert.AreEqual(new Vector3(2.0, 4.0, 6.0), v5);
            var v6 = v1 / 2.0;
            Assert.AreEqual(new Vector3(0.5, 1.0, 1.5), v6);
            Assert.IsTrue(v1 == new Vector3(1.0, 2.0, 3.0));
            Assert.IsFalse(v1 != new Vector3(1.0, 2.0, 3.0));
            Assert.IsFalse(v1 == v2);
            Assert.IsTrue(v1 != v2);
        }
        //Lengthメソッドのテスト
        [TestMethod]
        public void LengthTest()
        {
            var v1 = new Vector3(3.0, 4.0, 0.0);
            Assert.AreEqual(5.0, v1.Length());
            var v2 = new Vector3(0.0, 0.0, 0.0);
            Assert.AreEqual(0.0, v2.Length());
        }
        //Normalizeメソッドのテスト
        [TestMethod]
        public void NormalizeTest()
        {
            Vector3 v1 = new Vector3(3.0, 4.0, 0.0);
            Vector3 normalizedV1 = Vector3.Normalize(v1);
            Assert.AreEqual(new Vector3(0.6, 0.8, 0.0), normalizedV1);
            Assert.AreEqual(1.0, normalizedV1.Length());

            Vector3 v2 = new Vector3(0.0, 0.0, 0.0);
            Vector3 normalizedV2 = Vector3.Normalize(v2);
            Assert.AreEqual(new Vector3(0.0, 0.0, 0.0), normalizedV2);
        }

    }
}
