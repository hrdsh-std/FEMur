using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;


namespace FEMurTests
{
    [TestClass]
    public class Point3Test
    {
        [TestMethod]
        public void Operator()
        {
            Point3 a = new Point3(1, 2, 3);
            Point3 b = new Point3(4, 5, 6);
            Point3 c = a + b;
            Point3 expected = new Point3(5, 7, 9);
            Assert.AreEqual(expected, c);

            Point3 d = a - b;
            Point3 expectedSub = new Point3(-3, -3, -3);
            Assert.AreEqual(expectedSub, d);

            Point3 e = a * 2.0;
            Point3 expectedMul = new Point3(2, 4, 6);
            Assert.AreEqual(expectedMul, e);

            Point3 f = a / 2.0;
            Point3 expectedDiv = new Point3(0.5, 1, 1.5);
            Assert.AreEqual(expectedDiv, f);
        }
        //Equalsメソッドが正しく動作することを確認する
        [TestMethod]
        public void EqualsTest()
        {
            Point3 a = new Point3(1, 2, 3);
            Point3 b = new Point3(1, 2, 3);
            Point3 c = new Point3(4, 5, 6);
            Assert.IsTrue(a.Equals(b));
            Assert.IsFalse(a.Equals(c));   
        }
        //CompareToメソッドが正しく動作することを確認する
        [TestMethod]
        public void CompareToTest()
        {
            Point3 a = new Point3(1, 2, 3);
            Point3 b = new Point3(4, 5, 6);
            Point3 c = new Point3(1, 2, 3);
            Assert.IsTrue(a.CompareTo(b) < 0); // a < b
            Assert.IsTrue(b.CompareTo(a) > 0); // b > a
            Assert.IsTrue(a.CompareTo(c) == 0); // a == c
        }

    }
}
