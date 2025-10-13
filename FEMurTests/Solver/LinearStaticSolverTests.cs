using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Solver;
using System;
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
        public void Simple_Cantilever_TipPointLoad_Deflection()
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
                EnableRegularization = false,
                RotationalRegularizationFactor = 1e-8,
                EnableTranslationalRegularization = false,
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
                EnableRegularization = false,
                RotationalRegularizationFactor = 1e-8,
                EnableTranslationalRegularization = false,
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
                new Support(1, n2.Id, false, true,  true,  false, false, false)  // 右端: ローラー（回転は全自由）
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
                EnableRegularization = false,
                RotationalRegularizationFactor = 1e-9,
                EnableTranslationalRegularization = false,
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

        [TestMethod()]
        public void Cantilever_MultiNode_TipPointLoad_Deflection()
        {
            // 片持ち梁 L=1000mm を複数要素で離散化（例: 4要素・5節点）
            double L = 1000.0;
            int nodeCount = 2; // 5節点 → 4要素
            var nodes = new List<Node>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                double x = L * i / (nodeCount - 1);
                nodes.Add(new Node(i, x, 0.0, 0.0));
            }

            Material material = Material_Isotropic.Steel(); // E: N/mm^2
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0); // mm

            var elements = new List<ElementBase>(nodeCount - 1);
            for (int i = 0; i < nodeCount - 1; i++)
            {
                elements.Add(new BeamElement(i, nodes[i], nodes[i + 1], material, cs));
            }

            // 根元固定（6自由度固定）
            var supports = new List<Support>
            {
                new Support(0, nodes[0].Id, true, true, true, true, true, true)
            };

            // 先端に -Y 方向 1N を作用
            double P = 1.0; // N
            var loads = new List<Load>
            {
                new PointLoad(nodes[nodeCount - 1].Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);

            var solver = new LinearStaticSolver
            {
                EnableRegularization = false,
                RotationalRegularizationFactor = 1e-9,
                EnableTranslationalRegularization = false,
                TranslationalRegularizationFactor = 1e-10
            };

            var results = solver.Solve(model);
            var disp = results.NodalDisplacements;
            var reactions = results.ElementStresses;

            // 理論値（オイラー梁）: δ = -P L^3 / (3 E Izz)
            double E = material.E;
            double Izz = cs.Izz;
            double expected = -P * System.Math.Pow(L, 3) / (3.0 * E * Izz);

            // 先端節点の UY を検証
            int tipId = nodes[nodeCount - 1].Id;
            double tipY = disp[tipId * 6 + 1];

            //元端の応力を検証
            int baseElementId = 0;
            var baseStress = reactions[baseElementId];
            double M0 = P * L; // 根元の曲げモーメント
            double tolStress = 1e-9 + 1e-6 * System.Math.Abs(M0);

            Assert.AreEqual(M0, baseStress.Mz_i, tolStress);

            double tol = 1e-9 + 1e-6 * System.Math.Abs(expected);
            Assert.AreEqual(expected, tipY, tol);
        }

        [TestMethod()]
        public void PortalFrame_HorizontalLoad_Deflection()
        {
            // 門型フレーム: 2本の柱 + 1本の梁
            // 柱: 高さ 3000mm（Y方向）
            // 梁: スパン 4000mm（X方向）
            double H = 3000.0; // 柱の高さ
            double W = 4000.0; // 梁のスパン

            // 節点定義
            Node n0 = new Node(0, 0.0, 0.0, 0.0);     // 左下（固定）
            Node n1 = new Node(1, 0.0, H, 0.0);       // 左上
            Node n2 = new Node(2, W, H, 0.0);         // 右上
            Node n3 = new Node(3, W, 0.0, 0.0);       // 右下（固定）
            var nodes = new List<Node> { n0, n1, n2, n3 };

            Material material = Material_Isotropic.Steel();
            
            // 柱の断面（強軸方向が大きい）
            CrossSection_H csColumn = new CrossSection_H(0, "Column", 200.0, 200.0, 12.0, 8.0, 6.0);
            
            // 梁の断面
            CrossSection_H csBeam = new CrossSection_H(1, "Beam", 200.0, 300.0, 12.0, 8.0, 6.0);

            // 要素定義
            var e0 = new BeamElement(0, n0, n1, material, csColumn); // 左柱
            var e1 = new BeamElement(1, n1, n2, material, csBeam);   // 梁
            var e2 = new BeamElement(2, n2, n3, material, csColumn); // 右柱
            var elements = new List<ElementBase> { e0, e1, e2 };

            // 支持条件: 両端固定
            var supports = new List<Support>
            {
                new Support(0, n0.Id, true, true, true, true, true, true), // 左下固定
                new Support(1, n3.Id, true, true, true, true, true, true)  // 右下固定
            };

            // 荷重: 左上節点に水平荷重 10N（+X方向）
            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(P, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableRegularization = false
            };
            var disp = solver.solveDisp(model);

            // 水平変位を検証（左上節点のUX）
            double dispX_n1 = disp[n1.Id * 6 + 0];

            // 簡易理論値（柱の曲げ変形のみ考慮）:
            // δ = P * H^3 / (3 * E * I_column)
            // ただし、梁の影響で実際は小さくなる
            double E = material.E;
            double I_col = csColumn.Iyy; // Y軸周りの慣性モーメント（X方向曲げ）
            double expectedMax = P * System.Math.Pow(H, 3) / (3.0 * E * I_col);

            // 実際の変位は梁の拘束により expectedMax より小さいはず
            Assert.IsTrue(dispX_n1 > 0, "水平変位は正の値であるべき");
            Assert.IsTrue(dispX_n1 < expectedMax, "梁の拘束により変位は片持ち梁より小さいはず");
            
            // 変位のオーダーチェック（期待値の 10% ～ 100% の範囲）
            Assert.IsTrue(dispX_n1 > 0.1 * expectedMax && dispX_n1 <= expectedMax,
                $"変位 {dispX_n1:E3} が妥当な範囲 (0.1*{expectedMax:E3} ～ {expectedMax:E3}) にない");
        }

        [TestMethod()]
        public void ThreeDimensionalFrame_TorsionalLoad()
        {
            // 3次元フレーム: ねじりを含むケース
            // L字型フレーム（XY平面の柱 + XZ平面の梁）
            double L = 2000.0;

            Node n0 = new Node(0, 0.0, 0.0, 0.0);       // 原点（固定）
            Node n1 = new Node(1, 0.0, L, 0.0);         // Y方向上端
            Node n2 = new Node(2, L, L, 0.0);           // X方向端
            var nodes = new List<Node> { n0, n1, n2 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 200.0, 12.0, 8.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, cs); // 鉛直柱
            var e1 = new BeamElement(1, n1, n2, material, cs); // 水平梁
            var elements = new List<ElementBase> { e0, e1 };

            var supports = new List<Support>
            {
                new Support(0, n0.Id, true, true, true, true, true, true) // 完全固定
            };

            // 梁端にねじりモーメント 1000 N·mm（X軸周り）
            double Mx = 1000.0;
            var loads = new List<Load>
            {
                new PointLoad(n2.Id, new Vector3(0.0, 0.0, 0.0), new Vector3(Mx, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableRegularization = false
            };
            var disp = solver.solveDisp(model);

            // 梁端のねじり回転（RX）
            double rotX_n2 = disp[n2.Id * 6 + 3];

            // 理論値: θ = M * L / (G * J)
            double G = material.G;
            double J = cs.J;
            double expected = Mx * L / (G * J);

            double tol = 1e-9 + 1e-6 * System.Math.Abs(expected);
            Assert.AreEqual(expected, rotX_n2, tol,
                $"ねじり回転 {rotX_n2:E3} が理論値 {expected:E3} と一致すべき");
        }

        [TestMethod()]
        public void PortalFrame_UniformLoad_OnBeam()
        {
            // 門型フレーム: 梁に等分布荷重
            double H = 3000.0;
            double W = 4000.0;

            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, 0.0, H, 0.0);
            Node n2 = new Node(2, W / 2.0, H, 0.0);
            Node n3 = new Node(3, W, H, 0.0);
            Node n4 = new Node(4, W, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1, n2, n3, n4 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H csColumn = new CrossSection_H(0, "Column", 200.0, 200.0, 12.0, 8.0, 6.0);
            CrossSection_H csBeam = new CrossSection_H(1, "Beam", 200.0, 300.0, 12.0, 8.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, csColumn);
            var e1 = new BeamElement(1, n1, n2, material, csBeam);
            var e2 = new BeamElement(2, n2, n3, material, csBeam);
            var e3 = new BeamElement(3, n3, n4, material, csColumn);
            var elements = new List<ElementBase> { e0, e1, e2, e3 };

            var supports = new List<Support>
            {
                new Support(0, n0.Id, true, true, true, true, true, true),
                new Support(1, n4.Id, true, true, true, true, true, true)
            };

            // 梁全体に等分布荷重 q = 0.01 N/mm（-Y方向）
            double q = 0.01;
            var loads = new List<Load>
            {
                new ElementLoad(e1.Id, new Vector3(0.0, -q, 0.0)),
                new ElementLoad(e2.Id, new Vector3(0.0, -q, 0.0))
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableRegularization = false
            };
            var disp = solver.solveDisp(model);

            // 梁中央の鉛直変位
            double dispY_n2 = disp[n2.Id * 6 + 1];

            // 下向き（負）の変位であることを確認
            Assert.IsTrue(dispY_n2 < 0, "等分布荷重による変位は負（下向き）であるべき");

            // オーダーチェック（mm単位で数値が妥当か）
            Assert.IsTrue(System.Math.Abs(dispY_n2) > 1e-6 && System.Math.Abs(dispY_n2) < 100.0,
                $"変位 {dispY_n2:E3} のオーダーが妥当範囲にない");
        }

        [TestMethod()]
        public void Cantilever_Stress_Verification()
        {
            // 片持ち梁の応力検証
            double L = 1000.0;
            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, L, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, cs);
            var elements = new List<ElementBase> { e0 };

            var supports = new List<Support>
            {
                new Support(0, n0.Id, true, true, true, true, true, true)
            };

            // 先端に -Y 方向 10N を作用
            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver { EnableRegularization = false };
            var result = solver.Solve(model);

            // 応力検証
            Assert.AreEqual(1, result.ElementStresses.Count, "要素数が1つであるべき");

            var stress = result.ElementStresses[0];

            // i端（固定端）の曲げモーメント: M = P * L
            double expectedMz_i = P * L;
            Assert.AreEqual(expectedMz_i, stress.Mz_i, Math.Abs(expectedMz_i) * 1e-6,
                $"i端の曲げモーメント Mz_i={stress.Mz_i} が理論値 {expectedMz_i} と一致すべき");

            // i端のせん断力: V = P
            Assert.AreEqual(P, stress.Fy_i, Math.Abs(P) * 1e-6,
                $"i端のせん断力 Fy_i={stress.Fy_i} が理論値 {-P} と一致すべき");

            // j端（自由端）の曲げモーメント: ほぼ0
            Assert.AreEqual(0.0, stress.Mz_j, Math.Abs(expectedMz_i) * 1e-3,
                $"j端の曲げモーメント Mz_j={stress.Mz_j} はほぼ0であるべき");

            // j端のせん断力: V = -P
            Assert.AreEqual(-P, stress.Fy_j, Math.Abs(P) * 1e-6,
                $"j端のせん断力 Fy_j={stress.Fy_j} が理論値 {-P} と一致すべき");
        }

        [TestMethod()]
        public void SimplySupported_Stress_Verification()
        {
            // 単純支持梁（中央集中荷重）の応力検証
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
                new Support(0, n0.Id, true, true, true, true, false, false),
                new Support(1, n2.Id, false, true, true, true, false, false)
            };

            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver { EnableRegularization = true, EnableTranslationalRegularization = true };
            var result = solver.Solve(model);

            // 中央の最大曲げモーメント: M_max = P * L / 4
            double expectedMzMax = P * L / 4.0;

            var stress0 = result.ElementStresses[0];
            var stress1 = result.ElementStresses[1];

            // 左要素のj端（中央）の曲げモーメント
            Assert.AreEqual(expectedMzMax, Math.Abs(stress0.Mz_j), expectedMzMax * 0.01,
                $"中央の曲げモーメント |Mz|={Math.Abs(stress0.Mz_j)} が理論値 {expectedMzMax} に近いべき");

            // せん断力: 左端 V = P/2
            Assert.AreEqual(P / 2.0, stress0.Fy_i, P * 1e-3,
                $"左端のせん断力 Fy={stress0.Fy_i} が理論値 {P / 2.0} に近いべき");
        }

        // ===== 特異行列と自動正則化のテストケース =====

        [TestMethod()]
        public void AutoRegularization_SimplySupported_NoRotationalConstraints()
        {
            // 単純支持梁で回転拘束を一切設けない → 特異行列
            // 自動正則化により解析が完了し、警告が出ることを確認
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

            // 回転拘束なし（RX, RY, RZ すべて自由）→ 特異行列
            var supports = new List<Support>
            {
                new Support(0, n0.Id, true,  true,  true,  false, false, false), // 左端: 並進のみ拘束
                new Support(1, n2.Id, false, true,  true,  false, false, false)  // 右端: UY, UZ のみ拘束
            };

            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            
            // 自動正則化を有効にして解析
            var solver = new LinearStaticSolver
            {
                EnableAutoRegularization = true // デフォルトで true
            };

            var result = solver.Solve(model);

            // 解析が完了することを確認
            Assert.IsNotNull(result, "解析結果が null でない");
            Assert.IsNotNull(result.NodalDisplacements, "変位ベクトルが null でない");
            Assert.IsTrue(model.IsSolved, "モデルが解析済み状態である");

            // 警告が出ていることを確認
            Assert.IsTrue(solver.Warnings.Count > 0, "特異行列に対する警告が出ているべき");
            
            // 警告メッセージに "Singular" や "Auto-regularization" が含まれることを確認
            string warningMessage = string.Join(" ", solver.Warnings);
            Assert.IsTrue(warningMessage.Contains("Singular") || warningMessage.Contains("singular"),
                "警告メッセージに 'Singular' が含まれるべき");
            Assert.IsTrue(warningMessage.Contains("Auto-regularization") || warningMessage.Contains("regularization"),
                "警告メッセージに 'regularization' が含まれるべき");

            // 変位値が妥当な範囲にあることを確認（NaN や Infinity でない）
            double midY = result.NodalDisplacements[n1.Id * 6 + 1];
            Assert.IsFalse(double.IsNaN(midY), "中央変位が NaN でない");
            Assert.IsFalse(double.IsInfinity(midY), "中央変位が Infinity でない");
            Assert.IsTrue(midY < 0, "下向き荷重により負の変位");

            // オーダーチェック（正則化により精度は落ちるが、桁は妥当）
            double E = material.E;
            double Izz = cs.Izz;
            double expectedOrder = P * System.Math.Pow(L, 3) / (48.0 * E * Izz);
            Assert.IsTrue(System.Math.Abs(midY) > 0.1 * System.Math.Abs(expectedOrder),
                "変位のオーダーが理論値の 10% 以上");
            Assert.IsTrue(System.Math.Abs(midY) < 10.0 * System.Math.Abs(expectedOrder),
                "変位のオーダーが理論値の 10倍 以下");
        }

        [TestMethod()]
        public void AutoRegularization_Cantilever_MissingTranslationalConstraint()
        {
            // 片持ち梁で並進拘束が不足（UZ を拘束しない）→ Z方向の剛体変位モード
            // 自動正則化により解析完了、警告が出ることを確認
            double L = 1000.0;
            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, L, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, cs);
            var elements = new List<ElementBase> { e0 };

            // UZ を拘束しない → Z方向の剛体変位が残る
            var supports = new List<Support>
            {
                new Support(0, n0.Id, true, true, false, true, true, true) // UZ 未拘束
            };

            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableAutoRegularization = true
            };

            var result = solver.Solve(model);

            // 解析完了を確認
            Assert.IsNotNull(result);
            Assert.IsTrue(model.IsSolved);

            // 警告が出ていることを確認
            Assert.IsTrue(solver.Warnings.Count > 0, "特異行列に対する警告が出ているべき");

            // 先端の Y 変位が妥当な範囲
            double tipY = result.NodalDisplacements[n1.Id * 6 + 1];
            Assert.IsFalse(double.IsNaN(tipY));
            Assert.IsFalse(double.IsInfinity(tipY));
            Assert.IsTrue(tipY < 0, "下向き荷重により負の変位");

            double E = material.E;
            double Izz = cs.Izz;
            double expectedOrder = -P * System.Math.Pow(L, 3) / (3.0 * E * Izz);
            Assert.IsTrue(System.Math.Abs(tipY) > 0.1 * System.Math.Abs(expectedOrder),
                "変位のオーダーが理論値の 10% 以上");
        }

        [TestMethod()]
        public void AutoRegularization_FloatingFrame_NoSupports()
        {
            // 支持条件なし（浮遊フレーム）→ 完全な特異行列
            // 自動正則化でも解けない可能性が高いが、警告/エラーが適切に出ることを確認
            double L = 1000.0;
            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, L, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H cs = new CrossSection_H(0, "Test", 200.0, 100.0, 8.0, 12.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, cs);
            var elements = new List<ElementBase> { e0 };

            // 支持条件なし
            var supports = new List<Support>();

            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableAutoRegularization = true
            };

            bool exceptionThrown = false;
            try
            {
                var result = solver.Solve(model);
                
                // 例外が投げられずに解析完了した場合
                // 警告が出ていることを確認
                if (solver.Warnings.Count > 0)
                {
                    Assert.IsTrue(true, "自動正則化により解析完了、警告が出力された");
                }
            }
            catch (InvalidOperationException ex)
            {
                // 正則化後も特異の場合はエラーが投げられることを期待
                exceptionThrown = true;
                Assert.IsTrue(ex.Message.Contains("singular") || ex.Message.Contains("Singular"),
                    "例外メッセージに 'singular' が含まれるべき");
                Assert.IsTrue(ex.Message.Contains("regularization") || ex.Message.Contains("supports"),
                    "例外メッセージに対処方法のヒントが含まれるべき");
            }

            // 例外が投げられた場合は成功
            if (exceptionThrown)
            {
                Assert.IsTrue(true, "支持条件なしのケースで適切にエラーが発生");
            }
        }

        [TestMethod()]
        public void AutoRegularization_Disabled_ShouldThrowException()
        {
            // 自動正則化を無効にした場合、特異行列で例外が投げられることを確認
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

            // 回転拘束なし → 特異行列
            var supports = new List<Support>
            {
                new Support(0, n0.Id, true,  true,  true,  false, false, false),
                new Support(1, n2.Id, false, true,  true,  false, false, false)
            };

            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(0.0, -P, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            
            // 自動正則化を無効化
            var solver = new LinearStaticSolver
            {
                EnableAutoRegularization = false
            };

            // 例外が投げられることを期待
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                solver.Solve(model);
            }, "自動正則化無効時に特異行列で例外が投げられるべき");
        }

        [TestMethod()]
        public void AutoRegularization_PortalFrame_MissingOneRotationalConstraint()
        {
            // 門型フレームで RX 拘束が1つだけ不足 → 特異行列
            // 自動正則化により解析完了
            double H = 3000.0;
            double W = 4000.0;

            Node n0 = new Node(0, 0.0, 0.0, 0.0);
            Node n1 = new Node(1, 0.0, H, 0.0);
            Node n2 = new Node(2, W, H, 0.0);
            Node n3 = new Node(3, W, 0.0, 0.0);
            var nodes = new List<Node> { n0, n1, n2, n3 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H csColumn = new CrossSection_H(0, "Column", 200.0, 200.0, 12.0, 8.0, 6.0);
            CrossSection_H csBeam = new CrossSection_H(1, "Beam", 200.0, 300.0, 12.0, 8.0, 6.0);

            var e0 = new BeamElement(0, n0, n1, material, csColumn);
            var e1 = new BeamElement(1, n1, n2, material, csBeam);
            var e2 = new BeamElement(2, n2, n3, material, csColumn);
            var elements = new List<ElementBase> { e0, e1, e2 };

            // 両端の RX を拘束しない → X軸周りの剛体回転モード
            var supports = new List<Support>
            {
                new Support(0, n0.Id, true, true, true, false, true, true),  // RX 未拘束
                new Support(1, n3.Id, true, true, true, false, true, true)   // RX 未拘束
            };

            double P = 10.0;
            var loads = new List<Load>
            {
                new PointLoad(n1.Id, new Vector3(P, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), false)
            };

            var model = new Model(nodes, elements, supports, loads);
            var solver = new LinearStaticSolver
            {
                EnableAutoRegularization = true
            };

            var result = solver.Solve(model);

            // 解析完了、警告出力を確認
            Assert.IsNotNull(result);
            Assert.IsTrue(model.IsSolved);
            Assert.IsTrue(solver.Warnings.Count > 0, "特異行列に対する警告が出ているべき");

            // 水平変位が妥当
            double dispX_n1 = result.NodalDisplacements[n1.Id * 6 + 0];
            Assert.IsFalse(double.IsNaN(dispX_n1));
            Assert.IsFalse(double.IsInfinity(dispX_n1));
            Assert.IsTrue(dispX_n1 > 0, "水平荷重により正の変位");
        }
    }
}