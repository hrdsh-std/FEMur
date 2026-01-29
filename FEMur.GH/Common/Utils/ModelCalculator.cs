using System;
using FEMur.Models;

namespace FEMurGH.Common.Utils
{
    /// <summary>
    /// モデルに関する計算ユーティリティ
    /// </summary>
    public static class ModelCalculator
    {
        /// <summary>
        /// モデルの特性寸法を計算（バウンディングボックスの対角線長さ）
        /// </summary>
        /// <param name="model">FEMurモデル</param>
        /// <returns>特性寸法（対角線長さ）。節点が存在しない場合は0.0</returns>
        public static double CalculateCharacteristicLength(Model model)
        {
            if (model == null || model.Nodes == null || model.Nodes.Count == 0)
                return 0.0;

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (var node in model.Nodes)
            {
                minX = Math.Min(minX, node.Position.X);
                minY = Math.Min(minY, node.Position.Y);
                minZ = Math.Min(minZ, node.Position.Z);
                maxX = Math.Max(maxX, node.Position.X);
                maxY = Math.Max(maxY, node.Position.Y);
                maxZ = Math.Max(maxZ, node.Position.Z);
            }

            double dx = maxX - minX;
            double dy = maxY - minY;
            double dz = maxZ - minZ;

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// 力の自動スケールを計算
        /// </summary>
        /// <param name="model">FEMurモデル</param>
        /// <param name="maxForce">最大の力の値</param>
        /// <param name="scaleFactor">スケール係数（デフォルト: 0.15 = モデルサイズの15%）</param>
        /// <returns>自動スケール値</returns>
        public static double CalculateForceAutoScale(Model model, double maxForce, double scaleFactor = 0.15)
        {
            if (model == null || maxForce <= 0)
                return 1.0;

            double modelSize = CalculateCharacteristicLength(model);
            if (modelSize <= 0)
                return 1.0;

            return (modelSize * scaleFactor) / maxForce;
        }

        /// <summary>
        /// モーメントの自動スケールを計算
        /// </summary>
        /// <param name="model">FEMurモデル</param>
        /// <param name="maxMoment">最大のモーメントの値</param>
        /// <param name="scaleFactor">スケール係数（デフォルト: 0.1 = モデルサイズの10%）</param>
        /// <returns>自動スケール値</returns>
        public static double CalculateMomentAutoScale(Model model, double maxMoment, double scaleFactor = 0.1)
        {
            if (model == null || maxMoment <= 0)
                return 1.0;

            double modelSize = CalculateCharacteristicLength(model);
            if (modelSize <= 0)
                return 1.0;

            return (modelSize * scaleFactor) / maxMoment;
        }
    }
}