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
        }
    }
}
