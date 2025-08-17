using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace FEMurTests
{
    [TestClass]
    public class Face3Test
    {
        //Face3のEqualsメソッドのテスト
        [TestMethod]
        public void EqualsTest()
        {
            var face1 = new Face3(1, 2, 3);
            var face2 = new Face3(1, 2, 3);
            var face3 = new Face3(1, 2, 4);
            Assert.IsTrue(face1.Equals(face2));
            Assert.IsFalse(face1.Equals(face3));

            var face4 = new Face3(1, 2, 3, 4);
            var face5 = new Face3(1, 2, 3, 4);
            var face6 = new Face3(1, 2, 3, 5);
            Assert.IsTrue(face4.Equals(face5));
            Assert.IsFalse(face4.Equals(face6));
        }
        //Edgesメソッドのテスト
        [TestMethod]
        public void EdgesTest()
        {
            Face3 face = new Face3(1, 2, 3);
            IEnumerable<Edge3> edges = face.Edges();
            IEnumerable<Edge3> expectedEdges = new Edge3[]
            {
                new Edge3(1, 2),
                new Edge3(2, 3),
                new Edge3(3, 1)
            };
            for(int i = 0; i < edges.Count(); i++)
            {
                Assert.AreEqual(expectedEdges.ElementAt(i), edges.ElementAt(i));
            }            
        }
        //IsQuadプロパティのテスト
        [TestMethod]
        public void IsQuadTest()
        {
            Face3 triangleFace = new Face3(1, 2, 3);
            Assert.IsFalse(triangleFace.IsQuad);
            Assert.IsTrue(triangleFace.IsTriangle);
            Face3 quadFace = new Face3(1, 2, 3, 4);
            Assert.IsTrue(quadFace.IsQuad);
            Assert.IsFalse(quadFace.IsTriangle);
        }
        //IsTriangleプロパティのテスト
        [TestMethod]
        public void IsTriangleTest()
        {
            Face3 triangleFace = new Face3(1, 2, 3);
            Assert.IsTrue(triangleFace.IsTriangle);
            Assert.IsFalse(triangleFace.IsQuad);
            Face3 quadFace = new Face3(1, 2, 3, 4);
            Assert.IsFalse(quadFace.IsTriangle);
            Assert.IsTrue(quadFace.IsQuad);
        }
        //operator == と != のテスト
        [TestMethod]
        public void OperatorTest()
        {
            Face3 face1 = new Face3(1, 2, 3);
            Face3 face2 = new Face3(1, 2, 3);
            Face3 face3 = new Face3(1, 2, 4);
            Assert.IsTrue(face1 == face2);
            Assert.IsFalse(face1 != face2);
            Assert.IsFalse(face1 == face3);
            Assert.IsTrue(face1 != face3);
        }
    }
}
