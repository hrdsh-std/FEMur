using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Geometry;

namespace FEMurTests
{
    [TestClass]
    public class Mesh3Test
    {
        [TestMethod]
        //Equalsメソッドのテスト
        public void EqualsTest()
        {
            var mesh1 = new Mesh3();
            var mesh2 = new Mesh3();
            var mesh3 = new Mesh3();
            // 同じ内容のメッシュを作成
            mesh1.AddVertex(new Point3(0.0, 0.0, 0.0));//0
            mesh1.AddVertex(new Point3(1.0, 0.0, 0.0));//1
            mesh1.AddVertex(new Point3(0.0, 1.0, 0.0));//2
            mesh1.AddFace(new Face3(0, 1, 2));
            mesh2 = mesh1.Copy();
            // 異なる内容のメッシュを作成
            mesh3.AddVertex(new Point3(0.0, 0.0, 0.0));
            mesh3.AddVertex(new Point3(1.0, 0.0, 0.0));
            mesh3.AddVertex(new Point3(0.0, 1.0, 1.0)); // z座標が異なる
            mesh3.AddFace(new Face3(0, 1, 2));
            Assert.IsTrue(mesh1.Equals(mesh2));
            Assert.IsFalse(mesh1.Equals(mesh3));
        }
        //setVertexメソッドのテスト
        [TestMethod]
        public void SetVertexTest()
        {
            var mesh = new Mesh3();
            mesh.AddVertex(new Point3(0.0, 0.0, 0.0));
            mesh.AddVertex(new Point3(1.0, 0.0, 0.0));
            mesh.AddVertex(new Point3(0.0, 1.0, 0.0));
            mesh.SetVertex(1, new Point3(2.0, 2.0, 2.0));
            Assert.AreEqual(new Point3(2.0, 2.0, 2.0), mesh.GetVertex(1));
        }
        //GetFaceメソッドのテスト
        [TestMethod]
        public void GetFaceTest()
        {
            var mesh = new Mesh3();
            mesh.AddVertex(new Point3(0.0, 0.0, 0.0));
            mesh.AddVertex(new Point3(1.0, 0.0, 0.0));
            mesh.AddVertex(new Point3(0.0, 1.0, 0.0));
            mesh.AddFace(new Face3(0, 1, 2));
            var face = mesh.GetFace(0);
            Assert.AreEqual(new Face3(0, 1, 2), face);
        }
    }
}
