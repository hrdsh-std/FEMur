using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Models;
using FEMur.Nodes;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.CrossSections;
using FEMur.Supports;
using FEMur.Loads;
using FEMur.Joints;
using FEMur.Solver;
using FEMur.Geometry;

namespace FEMurTests.Solver
{
    [TestClass]
    public class JointTests
    {
        // 許容誤差
        private const double Tolerance = 1e-6;
        private const double StressTolerance = 1e-3;

        // 共通のマテリアルと断面
        private Material_Isotropic CreateSteel()
        {
            // E = 205000 N/mm^2, ν = 0.3
            return Material_Isotropic.Steel();
        }

        private CrossSection_Beam CreateRectSection()
        {
            // 100mm x 200mm の矩形断面
            return new CrossSection_Box(100, 200, 10);
        }

        #region 門型フレーム - 梁両端ピン接合テスト

        /// <summary>
        /// 門型フレームの梁両端をピン接合にすると、
        /// 柱は片持ち柱と同じ応力になることを確認
        /// 
        ///     P→ ●────────────● (梁: 両端ピン)
        ///        │            │
        ///        │ 柱1        │ 柱2
        ///        │            │
        ///        ▲            ▲ (固定支持)
        /// </summary>
        [TestMethod]
        public void PortalFrame_BeamWithPinJoints_ColumnsActAsCantilevers()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double columnHeight = 3000; // 柱高さ 3000mm
            double beamSpan = 4000;     // 梁スパン 4000mm
            double loadP = 10000.0;    // 水平荷重 10kN

            // 節点を作成
            var pt0 = new Point3(0, 0, 0);           // 柱1下端（固定）
            var pt1 = new Point3(0, 0, columnHeight); // 柱1上端 = 梁左端
            var pt2 = new Point3(beamSpan, 0, columnHeight); // 梁右端 = 柱2上端
            var pt3 = new Point3(beamSpan, 0, 0);    // 柱2下端（固定）

            // 要素を作成
            var column1 = new BeamElement(pt0, pt1, material, section); // ID=0
            var beam = new BeamElement(pt1, pt2, material, section);    // ID=1
            var column2 = new BeamElement(pt3, pt2, material, section); // ID=2

            var elements = new List<ElementBase> { column1, beam, column2 };

            // 支持条件：柱下端を固定
            var support0 = new Support(pt0, true, true, true, true, true, true);
            var support3 = new Support(pt3, true, true, true, true, true, true);
            var supports = new List<Support> { support0, support3 };

            // 荷重：柱1上端に水平荷重
            var load = new PointLoad(pt1, new Vector3(loadP, 0, 0), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint：梁（ID=1）の両端をピン接合
            var beamJoint = Joint.CreatePin(1);
            var joints = new List<Joint> { beamJoint };

            // モデルを作成
            var model = new Model(null, elements, supports, loads, joints);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            // 片持ち柱の理論解:
            // 曲げモーメント（柱下端）: M = P × H = 10000 × 3000 / 2= 15000000 N·m
            // せん断力: V = P = 5000 N

            // 柱1の断面力を確認 (ElementId = 0)
            var column1Stress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(column1Stress, "Column1 stress not found");

            // 柱1下端（i端）の曲げモーメント（強軸周り）
            // 座標系により符号が変わる可能性があるため、絶対値で比較
            double expectedMoment = loadP * columnHeight / 2; // 15000000 N·m
            double actualMoment_i = Math.Abs(column1Stress.My_i);
            Assert.AreEqual(expectedMoment, actualMoment_i, expectedMoment * 0.01,
                $"Column1 base moment: expected {expectedMoment}, actual {actualMoment_i}");

            // 柱1上端（j端）の曲げモーメントは0（ピン接合のため）
            double actualMoment_j = Math.Abs(column1Stress.My_j);
            Assert.AreEqual(0, actualMoment_j, StressTolerance,
                $"Column1 top moment should be 0 (pin joint), actual {actualMoment_j}");

            // せん断力
            double expectedShear = loadP / 2;
            double actualShear = Math.Abs(column1Stress.Fy_i);
            Assert.AreEqual(expectedShear, actualShear, expectedShear * 0.01,
                $"Column1 shear: expected {expectedShear}, actual {actualShear}");

            // 梁の両端モーメントも0であることを確認
            var beamStress = result.ElementStresses.Find(s => s.ElementId == 1);
            Assert.IsNotNull(beamStress, "Beam stress not found");
            Assert.AreEqual(0, Math.Abs(beamStress.My_i), StressTolerance,
                $"Beam left moment should be 0, actual {beamStress.My_i}");
            Assert.AreEqual(0, Math.Abs(beamStress.My_j), StressTolerance,
                $"Beam right moment should be 0, actual {beamStress.My_j}");
        }

        /// <summary>
        /// 比較用：門型フレームで梁が剛結の場合（Jointなし）
        /// 柱にはモーメントが分配される
        /// </summary>
        [TestMethod]
        public void PortalFrame_BeamRigid_MomentDistributed()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double columnHeight = 3.0;
            double beamSpan = 4.0;
            double loadP = 10000.0;

            var pt0 = new Point3(0, 0, 0);
            var pt1 = new Point3(0, 0, columnHeight);
            var pt2 = new Point3(beamSpan, 0, columnHeight);
            var pt3 = new Point3(beamSpan, 0, 0);

            var column1 = new BeamElement(pt0, pt1, material, section);
            var beam = new BeamElement(pt1, pt2, material, section);
            var column2 = new BeamElement(pt3, pt2, material, section);

            var elements = new List<ElementBase> { column1, beam, column2 };

            var support0 = new Support(pt0, true, true, true, true, true, true);
            var support3 = new Support(pt3, true, true, true, true, true, true);
            var supports = new List<Support> { support0, support3 };

            var load = new PointLoad(pt1, new Vector3(loadP, 0, 0), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint なし（剛結）
            var model = new Model(null, elements, supports, loads, null);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            // 剛結の場合、梁端部にモーメントが発生する
            var beamStress = result.ElementStresses.Find(s => s.ElementId == 1);
            Assert.IsNotNull(beamStress, "Beam stress not found");

            // 梁端部のモーメントは0ではない（剛結のため）
            double beamMoment_i = Math.Abs(beamStress.My_i);
            Assert.IsTrue(beamMoment_i > StressTolerance,
                $"Beam moment at rigid joint should be non-zero, actual {beamMoment_i}");
        }

        #endregion

        #region 単純梁 - 両端ピン接合テスト（集中荷重）

        /// <summary>
        /// 両端固定の単純梁に Joint で両端ピンを設定すると、
        /// 両端ピンの単純梁と同じ応力になることを確認
        /// 
        /// 理論解（中央集中荷重 P、スパン L）:
        /// - 両端固定: 端部モーメント M = PL/8, 中央モーメント M = PL/8
        /// - 両端ピン: 端部モーメント M = 0, 中央モーメント M = PL/4
        /// 
        ///           P (集中荷重)
        ///           ↓
        ///    ●──────●──────● (両端ピン)
        ///    △             △
        /// </summary>
        [TestMethod]
        public void SimpleBeam_WithPinJoints_CentralLoad()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double span = 6.0;        // スパン 6m
            double P = 30000.0;       // 中央集中荷重 30kN

            // 節点を作成（3分割して中央に節点を配置）
            var pt0 = new Point3(0, 0, 0);
            var ptMid = new Point3(span / 2, 0, 0);
            var pt1 = new Point3(span, 0, 0);

            // 要素を作成（2要素に分割）
            var beam1 = new BeamElement(pt0, ptMid, material, section);    // ID=0
            var beam2 = new BeamElement(ptMid, pt1, material, section);    // ID=1

            var elements = new List<ElementBase> { beam1, beam2 };

            // 支持条件：両端を固定（並進のみ拘束、回転は自由)
            // Jointでピン接合を指定するため、両端の支持は並進のみ拘束
            var support0 = new Support(pt0, true, true, true, false, false, false);
            var support1 = new Support(pt1, true, true, true, false, false, false);
            var supports = new List<Support> { support0, support1 };

            // 荷重：中央に下向き集中荷重
            var load = new PointLoad(ptMid, new Vector3(0, 0, -P), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint：両方の要素の両端をピン接合
            var joint1 = Joint.CreatePin(0);
            var joint2 = Joint.CreatePin(1);
            var joints = new List<Joint> { joint1, joint2 };

            // モデルを作成
            var model = new Model(null, elements, supports, loads, joints);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            // 両端ピンの単純梁の理論解:
            // - 端部せん断力: V = P/2 = 15000 N
            // - 端部モーメント: M = 0

            var beam1Stress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(beam1Stress, "Beam1 stress not found");

            // 端部モーメントは0（ピン接合）
            Assert.AreEqual(0, Math.Abs(beam1Stress.My_i), StressTolerance,
                $"Left end moment should be 0, actual {beam1Stress.My_i}");
            Assert.AreEqual(0, Math.Abs(beam1Stress.My_j), StressTolerance,
                $"Right end moment should be 0, actual {beam1Stress.My_j}");

            // 端部せん断力
            double expectedShear = P / 2; // 15000 N
            Assert.AreEqual(expectedShear, Math.Abs(beam1Stress.Fz_i), expectedShear * 0.01,
                $"Left end shear: expected {expectedShear}, actual {beam1Stress.Fz_i}");
        }

        /// <summary>
        /// 比較用：両端固定梁（Jointなし）中央集中荷重
        /// </summary>
        [TestMethod]
        public void SimpleBeam_FixedFixed_CentralLoad()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double span = 6.0;
            double P = 30000.0;

            var pt0 = new Point3(0, 0, 0);
            var ptMid = new Point3(span / 2, 0, 0);
            var pt1 = new Point3(span, 0, 0);

            var beam1 = new BeamElement(pt0, ptMid, material, section);
            var beam2 = new BeamElement(ptMid, pt1, material, section);

            var elements = new List<ElementBase> { beam1, beam2 };

            // 両端を完全固定
            var support0 = new Support(pt0, true, true, true, true, true, true);
            var support1 = new Support(pt1, true, true, true, true, true, true);
            var supports = new List<Support> { support0, support1 };

            var load = new PointLoad(ptMid, new Vector3(0, 0, -P), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint なし（剛結）
            var model = new Model(null, elements, supports, loads, null);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            // 両端固定梁の理論解:
            // - 端部モーメント: M = PL/8 = 30000 × 6 / 8 = 22500 N·m

            var beam1Stress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(beam1Stress, "Beam1 stress not found");

            double expectedMoment = P * span / 8; // 22500 N·m

            // 端部モーメントは PL/8
            Assert.AreEqual(expectedMoment, Math.Abs(beam1Stress.My_i), expectedMoment * 0.01,
                $"Left end moment: expected {expectedMoment}, actual {beam1Stress.My_i}");
            Assert.AreEqual(expectedMoment, Math.Abs(beam1Stress.My_j), expectedMoment * 0.01,
                $"Right end moment: expected {expectedMoment}, actual {beam1Stress.My_j}");
        }

        /// <summary>
        /// 片端ピン、片端剛結の単純梁（中央集中荷重）
        /// </summary>
        [TestMethod]
        public void SimpleBeam_PinRigid_CentralLoad()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double span = 6.0;
            double P = 30000.0;

            var pt0 = new Point3(0, 0, 0);
            var ptMid = new Point3(span / 2, 0, 0);
            var pt1 = new Point3(span, 0, 0);

            var beam1 = new BeamElement(pt0, ptMid, material, section);
            var beam2 = new BeamElement(ptMid, pt1, material, section);

            var elements = new List<ElementBase> { beam1, beam2 };

            // 両端を固定支持
            var support0 = new Support(pt0, true, true, true, true, true, true);
            var support1 = new Support(pt1, true, true, true, true, true, true);
            var supports = new List<Support> { support0, support1 };

            var load = new PointLoad(ptMid, new Vector3(0, 0, -P), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint：左側要素の左端をピン、右側要素の右端を剛結
            var joint1 = Joint.CreatePinRigid(0);  // 左要素: 左端ピン、右端剛結
            var joint2 = Joint.CreateRigidPin(1);  // 右要素: 左端剛結、右端ピン
            var joints = new List<Joint> { joint1 };  // 左端のみピン

            var model = new Model(null, elements, supports, loads, joints);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            var beam1Stress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(beam1Stress, "Beam1 stress not found");

            // 左端（ピン）のモーメントは0
            Assert.AreEqual(0, Math.Abs(beam1Stress.My_i), StressTolerance,
                $"Left end (pin) moment should be 0, actual {beam1Stress.My_i}");

            // 右端（固定）のモーメントは0ではない
            Assert.IsTrue(Math.Abs(beam1Stress.My_j) > StressTolerance,
                $"Right end (rigid) moment should be non-zero, actual {beam1Stress.My_j}");
        }

        #endregion

        #region 片持ち梁テスト

        /// <summary>
        /// 片持ち梁の先端にピン接合を設定しても、
        /// 固定端のモーメントは変わらないことを確認
        /// （先端は自由端なので、ピン接合の効果なし）
        /// 
        ///    P (集中荷重)
        ///    ↓
        ///    ●────────────● (先端ピン)
        ///    ▼ (固定支持)
        /// </summary>
        [TestMethod]
        public void Cantilever_TipLoad_PinJointAtTip()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double length = 4.0;      // 長さ 4m
            double P = 10000.0;       // 先端集中荷重 10kN

            var pt0 = new Point3(0, 0, 0);  // 固定端
            var pt1 = new Point3(length, 0, 0);  // 先端

            var beam = new BeamElement(pt0, pt1, material, section);
            var elements = new List<ElementBase> { beam };

            // 固定支持
            var support = new Support(pt0, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            // 先端に下向き荷重
            var load = new PointLoad(pt1, new Vector3(0, 0, -P), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint：先端をピン接合（効果なしのはず）
            var joint = Joint.CreateRigidPin(0);
            var joints = new List<Joint> { joint };

            var model = new Model(null, elements, supports, loads, joints);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            // 片持ち梁の理論解:
            // - 固定端モーメント: M = P × L = 10000 × 4 = 40000 N·m
            // - 固定端せん断力: V = P = 10000 N

            var beamStress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(beamStress, "Beam stress not found");

            double expectedMoment = P * length; // 40000 N·m
            double expectedShear = P;

            // 固定端モーメント
            Assert.AreEqual(expectedMoment, Math.Abs(beamStress.My_i), expectedMoment * 0.01,
                $"Fixed end moment: expected {expectedMoment}, actual {beamStress.My_i}");

            // 固定端せん断力
            Assert.AreEqual(expectedShear, Math.Abs(beamStress.Fz_i), expectedShear * 0.01,
                $"Fixed end shear: expected {expectedShear}, actual {beamStress.Fz_i}");

            // 先端モーメントは0（ピン接合でも自由端でも同じ）
            Assert.AreEqual(0, Math.Abs(beamStress.My_j), StressTolerance,
                $"Tip moment should be 0, actual {beamStress.My_j}");
        }

        /// <summary>
        /// 片持ち梁の固定端にピン接合を設定すると、
        /// 不安定構造になることを確認（または微小モーメント）
        /// </summary>
        [TestMethod]
        public void Cantilever_PinAtFixedEnd_BecomesUnstable()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double length = 4.0;
            double P = 10000.0;

            var pt0 = new Point3(0, 0, 0);
            var pt1 = new Point3(length, 0, 0);

            var beam = new BeamElement(pt0, pt1, material, section);
            var elements = new List<ElementBase> { beam };

            var support = new Support(pt0, true, true, true, true, true, true);
            var supports = new List<Support> { support };

            var load = new PointLoad(pt1, new Vector3(0, 0, -P), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint：固定端をピン接合（不安定になる）
            var joint = Joint.CreatePinRigid(0);
            var joints = new List<Joint> { joint };

            var model = new Model(null, elements, supports, loads, joints);

            // Act
            var solver = new LinearStaticSolver();
            solver.EnableAutoRegularization = true; // 自動正則化を有効にして解析を試みる
            var result = solver.Solve(model);

            // Assert
            // 固定端がピンになると、回転を拘束する要素がなくなる
            // 自動正則化により解が得られるが、固定端のモーメントは0に近くなる
            var beamStress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(beamStress, "Beam stress not found");

            // 固定端（ピン化された）のモーメントは0に近い
            Assert.AreEqual(0, Math.Abs(beamStress.My_i), 100, // 正則化の影響で完全に0にはならない
                $"Pin end moment should be near 0, actual {beamStress.My_i}");
        }

        #endregion

        #region 半剛接合テスト

        /// <summary>
        /// 半剛接合（回転ばね）を設定した場合、
        /// ピンと剛結の中間の挙動になることを確認
        /// </summary>
        [TestMethod]
        public void SimpleBeam_SemiRigid_IntermediateBehavior()
        {
            // Arrange
            var material = CreateSteel();
            var section = CreateRectSection();

            double span = 6.0;
            double P = 30000.0;

            // 回転ばね剛性を計算（EI/L程度の値）
            double E = material.E;
            double I = section.Iyy;
            double L = span;
            double springStiffness = E * I / L; // 半剛接合の代表的な値

            var pt0 = new Point3(0, 0, 0);
            var ptMid = new Point3(span / 2, 0, 0);
            var pt1 = new Point3(span, 0, 0);

            var beam1 = new BeamElement(pt0, ptMid, material, section);
            var beam2 = new BeamElement(ptMid, pt1, material, section);

            var elements = new List<ElementBase> { beam1, beam2 };

            var support0 = new Support(pt0, true, true, true, true, true, true);
            var support1 = new Support(pt1, true, true, true, true, true, true);
            var supports = new List<Support> { support0, support1 };

            var load = new PointLoad(ptMid, new Vector3(0, 0, -P), new Vector3(0, 0, 0));
            var loads = new List<Load> { load };

            // Joint：両端を半剛接合（Y軸周りの回転ばね）
            var joint1 = Joint.CreateSemiRigid(0, 0, springStiffness, 0, 0, springStiffness, 0);
            var joints = new List<Joint> { joint1 };

            var model = new Model(null, elements, supports, loads, joints);

            // Act
            var solver = new LinearStaticSolver();
            var result = solver.Solve(model);

            // Assert
            var beam1Stress = result.ElementStresses.Find(s => s.ElementId == 0);
            Assert.IsNotNull(beam1Stress, "Beam stress not found");

            // 両端固定の端部モーメント: PL/8 = 22500 N·m
            double fixedMoment = P * span / 8;

            // 半剛接合の場合、端部モーメントは0より大きく、両端固定より小さい
            double actualMoment = Math.Abs(beam1Stress.My_i);

            Assert.IsTrue(actualMoment > StressTolerance,
                $"Semi-rigid moment should be > 0, actual {actualMoment}");
            Assert.IsTrue(actualMoment < fixedMoment,
                $"Semi-rigid moment ({actualMoment}) should be < fixed moment ({fixedMoment})");
        }

        #endregion
    }
}