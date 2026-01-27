using Microsoft.VisualStudio.TestTools.UnitTesting;
using FEMur.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using FEMur.Nodes;
using FEMur.Models;
using FEMur.Elements;
using FEMur.Materials;
using FEMur.Loads;
using FEMur.Supports;
using FEMur.CrossSections;
using FEMur.Geometry;
using FEMur.Results;

namespace FEMurTests.Solver
{
    [TestClass]
    public class ReactionForceTests
    {
        private const double Tolerance = 1e-6;

        /// <summary>
        /// テスト1: 片持ち梁の先端集中荷重
        /// 固定端の反力を検証
        /// </summary>
        [TestMethod]
        public void Test_CantileverBeam_PointLoad_ReactionForce()
        {
            // Arrange: 片持ち梁（X方向、長さ1000mm）
            Node node1 = new Node(0, 0.0, 0.0, 0.0);      // 固定端
            Node node2 = new Node(1, 1000.0, 0.0, 0.0);   // 自由端
            List<Node> nodes = new List<Node> { node1, node2 };

            Material material = Material_Isotropic.Steel();
            CrossSection_H crossSection = new CrossSection_H("Test", 200.0, 100.0, 8.0, 12.0, 6.0);

            BeamElement element = new BeamElement(0, node1, node2, material, crossSection);
            List<ElementBase> elements = new List<ElementBase> { element };

            // 根元完全固定
            Support support = new Support(0, true, true, true, true, true, true);
            List<Support> supports = new List<Support> { support };

            // 先端に下向き荷重 -1000N (Y方向)
            PointLoad load = new PointLoad(1, new Vector3(0.0, -1000.0, 0.0), new Vector3(0.0, 0.0, 0.0));
            List<Load> loads = new List<Load> { load };

            Model model = new Model(nodes, elements, supports, loads);

            // Act
            var solver = new LinearStaticSolver
            {
                EnableRegularization = false,
                EnableAutoRegularization = false
            };
            var result = solver.Solve(model);

            // Assert
            Assert.IsNotNull(result.ReactionForces, "ReactionForces should not be null");
            Assert.AreEqual(1, result.ReactionForces.Count, "Should have 1 reaction force at fixed support");

            var reaction = result.ReactionForces.First(r => r.NodeId == 0);

            Console.WriteLine("\n=== 片持ち梁の反力 ===");
            Console.WriteLine($"固定端反力:");
            Console.WriteLine($"  Fx = {reaction.Fx:F6} N");
            Console.WriteLine($"  Fy = {reaction.Fy:F6} N");
            Console.WriteLine($"  Fz = {reaction.Fz:F6} N");
            Console.WriteLine($"  Mx = {reaction.Mx:F6} N?mm");
            Console.WriteLine($"  My = {reaction.My:F6} N?mm");
            Console.WriteLine($"  Mz = {reaction.Mz:F6} N?mm");

            // 反力検証
            // Fy = +1000N (上向き、荷重に対抗)
            Assert.AreEqual(1000.0, reaction.Fy, Tolerance * 1000, "Vertical reaction force Fy should be 1000N");

            // Fx, Fz = 0 (水平方向の荷重なし)
            Assert.AreEqual(0.0, reaction.Fx, Tolerance, "Horizontal reaction force Fx should be 0");
            Assert.AreEqual(0.0, reaction.Fz, Tolerance, "Out-of-plane reaction force Fz should be 0");

            // Mz = 1000N × 1000mm = 1,000,000 N?mm (時計回りモーメント)
            double expectedMoment = 1000.0 * 1000.0;
            Assert.AreEqual(expectedMoment, reaction.Mz, Math.Abs(expectedMoment) * 0.01, 
                "Reaction moment Mz should be 1,000,000 N?mm");

            // Mx, My = 0 (XY平面内の曲げのみ)
            Assert.AreEqual(0.0, reaction.Mx, Tolerance, "Reaction moment Mx should be 0");
            Assert.AreEqual(0.0, reaction.My, Tolerance, "Reaction moment My should be 0");

            // 力の釣り合い確認
            Console.WriteLine("\n=== 力の釣り合い確認 ===");
            Console.WriteLine($"入力荷重: Fy = -1000 N");
            Console.WriteLine($"反力合計: Fy = {reaction.Fy:F6} N");
            Console.WriteLine($"誤差: {Math.Abs(reaction.Fy - 1000.0):E6} N");
        }

        /// <summary>
        /// テスト2: 柱脚ピンの平面ラーメンフレーム
        /// 両端ピン支持の門型フレームに水平荷重
        /// </summary>
        [TestMethod]
        public void Test_PinnedPortalFrame_HorizontalLoad_ReactionForce()
        {
            // Arrange: 門型フレーム
            // 
            //     3 -------- 4 -------- 5
            //     |                     |
            //     |                     |
            //     1                     2
            //     ▲(ピン)              ▲(ピン)
            //
            // 寸法: 柱高さ3000mm, 梁スパン6000mm
            // 節点4に水平荷重10kN (X方向)

            double columnHeight = 3000.0; // mm
            double beamSpan = 6000.0;     // mm

            Node node1 = new Node(0, 0.0, 0.0, 0.0);                    // 左柱脚
            Node node2 = new Node(1, beamSpan, 0.0, 0.0);               // 右柱脚
            Node node3 = new Node(2, 0.0, columnHeight, 0.0);           // 左柱頭
            Node node4 = new Node(3, beamSpan / 2.0, columnHeight, 0.0); // 梁中央
            Node node5 = new Node(4, beamSpan, columnHeight, 0.0);      // 右柱頭

            List<Node> nodes = new List<Node> { node1, node2, node3, node4, node5 };

            Material material = Material_Isotropic.Steel();
            
            // 柱: H-300x150x6.5x9
            CrossSection_H columnSection = new CrossSection_H("Column", 300.0, 150.0, 6.5, 9.0, 8.0);
            
            // 梁: H-400x200x8x13
            CrossSection_H beamSection = new CrossSection_H("Beam", 400.0, 200.0, 8.0, 13.0, 11.0);

            // 要素定義
            BeamElement leftColumn = new BeamElement(0, node1, node3, material, columnSection);   // 左柱
            BeamElement rightColumn = new BeamElement(1, node2, node5, material, columnSection);  // 右柱
            BeamElement beamLeft = new BeamElement(2, node3, node4, material, beamSection);       // 左梁
            BeamElement beamRight = new BeamElement(3, node4, node5, material, beamSection);      // 右梁

            List<ElementBase> elements = new List<ElementBase> 
            { 
                leftColumn, rightColumn, beamLeft, beamRight 
            };

            // 柱脚ピン支持（X,Y,Z固定、回転自由）
            Support supportLeft = new Support(0, true, true, true, false, false, false);
            Support supportRight = new Support(1, true, true, true, false, false, false);
            List<Support> supports = new List<Support> { supportLeft, supportRight };

            // 梁中央節点に水平荷重 10000N (X方向)
            PointLoad horizontalLoad = new PointLoad(3, new Vector3(10000.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0));
            List<Load> loads = new List<Load> { horizontalLoad };

            Model model = new Model(nodes, elements, supports, loads);

            // Act
            var solver = new LinearStaticSolver
            {
                EnableRegularization = false,
                EnableAutoRegularization = false
            };
            var result = solver.Solve(model);

            // Assert
            Assert.IsNotNull(result.ReactionForces, "ReactionForces should not be null");
            Assert.AreEqual(2, result.ReactionForces.Count, "Should have 2 reaction forces (left and right supports)");

            var reactionLeft = result.ReactionForces.First(r => r.NodeId == 0);
            var reactionRight = result.ReactionForces.First(r => r.NodeId == 1);

            Console.WriteLine("\n=== 門型フレームの反力 ===");
            Console.WriteLine($"左柱脚反力 (Node {reactionLeft.NodeId}):");
            Console.WriteLine($"  Fx = {reactionLeft.Fx:F6} N");
            Console.WriteLine($"  Fy = {reactionLeft.Fy:F6} N");
            Console.WriteLine($"  Fz = {reactionLeft.Fz:F6} N");
            Console.WriteLine($"  Mx = {reactionLeft.Mx:F6} N?mm");
            Console.WriteLine($"  My = {reactionLeft.My:F6} N?mm");
            Console.WriteLine($"  Mz = {reactionLeft.Mz:F6} N?mm");
            
            Console.WriteLine($"\n右柱脚反力 (Node {reactionRight.NodeId}):");
            Console.WriteLine($"  Fx = {reactionRight.Fx:F6} N");
            Console.WriteLine($"  Fy = {reactionRight.Fy:F6} N");
            Console.WriteLine($"  Fz = {reactionRight.Fz:F6} N");
            Console.WriteLine($"  Mx = {reactionRight.Mx:F6} N?mm");
            Console.WriteLine($"  My = {reactionRight.My:F6} N?mm");
            Console.WriteLine($"  Mz = {reactionRight.Mz:F6} N?mm");

            // 水平力の釣り合い: Fx_left + Fx_right = 10000N
            double totalFx = reactionLeft.Fx + reactionRight.Fx;
            Console.WriteLine("\n=== 水平力の釣り合い ===");
            Console.WriteLine($"入力荷重: Fx = 10000 N");
            Console.WriteLine($"左柱脚: Fx = {reactionLeft.Fx:F6} N");
            Console.WriteLine($"右柱脚: Fx = {reactionRight.Fx:F6} N");
            Console.WriteLine($"反力合計: Fx = {totalFx:F6} N");
            Console.WriteLine($"誤差: {Math.Abs(totalFx - 10000.0):E6} N");

            Assert.AreEqual(10000.0, totalFx, Math.Abs(totalFx) * 0.01, 
                "Total horizontal reaction should equal applied load");

            // 対称性により、両柱脚の水平反力は等しい（約5000Nずつ）
            Console.WriteLine("\n=== 対称性の確認 ===");
            Console.WriteLine($"左柱脚水平反力: {reactionLeft.Fx:F6} N");
            Console.WriteLine($"右柱脚水平反力: {reactionRight.Fx:F6} N");
            Console.WriteLine($"差分: {Math.Abs(reactionLeft.Fx - reactionRight.Fx):F6} N");

            Assert.AreEqual(5000.0, reactionLeft.Fx, 500.0, 
                "Left support should carry approximately 5000N (symmetric)");
            Assert.AreEqual(5000.0, reactionRight.Fx, 500.0, 
                "Right support should carry approximately 5000N (symmetric)");

            // 鉛直力はゼロ（鉛直荷重なし）
            Assert.AreEqual(0.0, reactionLeft.Fy, Tolerance * 100, 
                "Vertical reaction at left support should be 0");
            Assert.AreEqual(0.0, reactionRight.Fy, Tolerance * 100, 
                "Vertical reaction at right support should be 0");

            // Z方向の力もゼロ（平面フレーム）
            Assert.AreEqual(0.0, reactionLeft.Fz, Tolerance, 
                "Out-of-plane reaction Fz at left support should be 0");
            Assert.AreEqual(0.0, reactionRight.Fz, Tolerance, 
                "Out-of-plane reaction Fz at right support should be 0");

            // ピン支持なので回転モーメントは発生しない
            Console.WriteLine("\n=== ピン支持の確認（回転モーメント）===");
            Console.WriteLine($"左柱脚: Mx={reactionLeft.Mx:E6}, My={reactionLeft.My:E6}, Mz={reactionLeft.Mz:E6}");
            Console.WriteLine($"右柱脚: Mx={reactionRight.Mx:E6}, My={reactionRight.My:E6}, Mz={reactionRight.Mz:E6}");

            Assert.AreEqual(0.0, reactionLeft.Mx, Tolerance, 
                "Moment Mx at pinned support should be 0");
            Assert.AreEqual(0.0, reactionLeft.My, Tolerance, 
                "Moment My at pinned support should be 0");
            Assert.AreEqual(0.0, reactionLeft.Mz, Tolerance, 
                "Moment Mz at pinned support should be 0");
            
            Assert.AreEqual(0.0, reactionRight.Mx, Tolerance, 
                "Moment Mx at pinned support should be 0");
            Assert.AreEqual(0.0, reactionRight.My, Tolerance, 
                "Moment My at pinned support should be 0");
            Assert.AreEqual(0.0, reactionRight.Mz, Tolerance, 
                "Moment Mz at pinned support should be 0");

            Console.WriteLine("\n? すべての検証に合格しました");
        }
    }
}