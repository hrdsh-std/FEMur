using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Nodes;
using FEMur.Models;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.Loads;
using FEMur.Supports;
using FEMur.Results;
using MathNet.Numerics.LinearAlgebra;
using FEMur.CrossSections;
using FEMur.Geometry;


namespace FEMur.Solver.Tests
{
    [TestClass()]
    public class LinearStaticSolverTests
    {
        [TestMethod()]
        public void SolveTest()
        {
            //片持ち梁のモデルを作成
            Node node1 = new Node(0, 0.0, 0.0, 0.0);
            Node node2 = new Node(1, 10.0, 0.0, 0.0);
            List<Node> nodes = new List<Node> { node1, node2 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H crossSection = new CrossSection_H(0,"Test", 200.0, 100.0, 8.0, 12.0,6.0);

            BeamElement element1 = new BeamElement(0, node1, node2, material, crossSection);
            List<ElementBase> elements = new List<ElementBase> { element1 };

            Support support1 = new Support(0,node1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            List<Support> supports = new List<Support> { support1 };

            PointLoad load1 = new PointLoad(0,new Vector3(0.0,1.0,0.0),new Vector3(0.0,0.0,0.0),false);
            List<Load> loads = new List<Load> { load1 };

            Model model = new Model(nodes,elements,supports,loads);

            //線形静解析を実行
            LinearStaticSolver solver = new LinearStaticSolver();
            Result result = solver.Solve(model);
            //結果の検証
            double tol = 1e-6;
            //節点2のY方向変位
            double expectedDisplacementY = -0.0003030303; //理論値
            double actualDisplacementY = result.NodalDisplacements[4]; //節点2のY方向変位成分
            Assert.AreEqual(expectedDisplacementY, actualDisplacementY, tol);

        }
    }
}