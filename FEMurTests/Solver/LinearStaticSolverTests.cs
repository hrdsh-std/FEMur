using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Solver;
using System.Collections.Generic;
using FEMur.Nodes;
using FEMur.Models;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.Loads;
using FEMur.Supports;
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
            // 片持ち梁（X軸に沿って配置）
            Node node1 = new Node(0, 0.0, 0.0, 0.0);    // mm
            Node node2 = new Node(1, 1000.0, 0.0, 0.0); // 1000mm に変更（オーダー合わせ）
            List<Node> nodes = new List<Node> { node1, node2 };

            Material material = Material_Isotropic.Steel(); // E は N/mm^2 を仮定
            CrossSection_H crossSection = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0); // mm

            BeamElement element1 = new BeamElement(0, node1, node2, material, crossSection);
            List<ElementBase> elements = new List<ElementBase> { element1 };

            // 根元固定（6自由度固定）
            Support support1 = new Support(0, 0, true, true, true, true, true, true);
            List<Support> supports = new List<Support> { support1 };

            // 先端に -Y 方向 1N を作用
            PointLoad load1 = new PointLoad(1, new Vector3(0.0, -1.0, 0.0), new Vector3(0.0, 0.0, 0.0), false);
            List<Load> loads = new List<Load> { load1 };

            Model model = new Model(nodes, elements, supports, loads);

            var solver = new LinearStaticSolver
            {
                EnableRegularization = true,
                RotationalRegularizationFactor = 1e-8,
                EnableTranslationalRegularization = true,
                TranslationalRegularizationFactor = 1e-8
            };
            Vector<double> disp = solver.solveDisp(model);

            double L = node2.Position.X - node1.Position.X; // mm
            double E = material.E;                           // N/mm^2
            double Izz = crossSection.Izz;                   // mm^4
            double Fy = -1.0;                                // N
            double expectedDisplacementY = Fy * (L * L * L) / (3.0 * E * Izz); // mm

            double DispY = disp[7];

            double tol = 1e-9 + 1e-6 * System.Math.Abs(expectedDisplacementY);
            Assert.AreEqual(expectedDisplacementY, DispY, tol);
        }

        [TestMethod()]
        public void SimplySupported_PointLoad_AtCenter_Deflection()
        {
            // 単純支持梁 L=1000mm
            double L = 1000.0;
            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, L / 2.0, 0.0, 0.0); // 中間節点
            Node n2 = new Node(2, L, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1, n2 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, cs);
            var e1 = new BeamElement(1, n1, n2, material, cs);
            var elements = new List<ElementBase> { e0, e1 };

            // 単純支持: 左端ピン（UX, UY, UZ, さらに RX を1点拘束）, 右端ローラー（UY, UZ）
            var supports = new List<Support>
            {
                // Support(id, nodeId, fixDX, fixDY, fixDZ, fixRX, fixRY, fixRZ)
                new Support(0, n0.Id, true,  true,  true,  true,  false, false), // 左端: RX 1点拘束
                new Support(1, n2.Id, false, true,  true,  true, false, false)  // 右端: ローラー（回転は全自由）
            };

            double P = 1.0; // N
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableRegularization = true,
                RotationalRegularizationFactor = 1e-8,
                EnableTranslationalRegularization = true,
                TranslationalRegularizationFactor = 1e-8
            };
            var disp = solver.solveDisp(model);

            double E = material.E;
            double Izz = cs.Izz;
            double expected = -P * System.Math.Pow(L, 3) / (48.0 * E * Izz);

            double midY = disp[n1.Id * 6 + 1];
            double tol = 1e-9 + 1e-6 * System.Math.Abs(expected);
            Assert.AreEqual(expected, midY, tol);
        }

        [TestMethod()]
        public void SimplySupported_UDL_Deflection_ElementLoad()
        {
            // 単純支持梁 L=1000mm、2要素
            double L = 1000.0;
            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, L / 2.0, 0.0, 0.0);
            Node n2 = new Node(2, L, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1, n2 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, cs);
            var e1 = new BeamElement(1, n1, n2, material, cs);
            var elements = new List<ElementBase> { e0, e1 };

            var supports = new List<Support>
            {
                new Support(0, n0.Id, true,  true,  true,  true,  false, false), // 左端: RX だけ追加で固定
                new Support(1, n2.Id, false, true,  true,  true, false, false)  // 右端: ローラー（回転は全自由）
            };

            // 等分布荷重 q [N/mm]（全長に -Y 方向）
            double q = 0.01;

            // 各要素にローカル-Y方向の等分布荷重を付与（Local: true）
            var loads = new List<Load>
            {
                new ElementLoad(e0.Id, new Vector3(0.0, -q, 0.0)),
                new ElementLoad(e1.Id, new Vector3(0.0, -q, 0.0))
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableRegularization = true,
                RotationalRegularizationFactor = 1e-9,
                EnableTranslationalRegularization = true,
                TranslationalRegularizationFactor = 1e-10
            };
            var disp = solver.solveDisp(model);

            // 理論値: δmax = -5 q L^4 / (384 E Izz) （中央）
            double E = material.E;
            double Izz = cs.Izz;
            double expected = -5.0 * q * System.Math.Pow(L, 4) / (384.0 * E * Izz);

            double midY = disp[n1.Id * 6 + 1];
            double tol = 1e-9 + 1e-6 * System.Math.Abs(expected);
            Assert.AreEqual(expected, midY, tol);
        }
    }
}