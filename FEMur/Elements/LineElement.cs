using FEMur.CrossSections;
using FEMur.Geometry;
using FEMur.Materials;
using FEMur.Nodes;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FEMur.Elements
{
    //abstract class for line elements such as beams, trusses, etc.
    public abstract class LineElement : ElementBase, ISerializable
    {
        public double BetaAngle { get; set; } = 0.0;

        public LineElement() { }

        // IDなしコンストラクタ（推奨）
        protected LineElement(Point3 point1, Point3 point2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(new List<Point3> { point1, point2 }, material, crossSection)
        {
            BetaAngle = betaAngle;
        }

        // IDなしコンストラクタ（NodeIds指定）
        protected LineElement(List<int> nodeIds, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(nodeIds, material, crossSection)
        {
            BetaAngle = betaAngle;
        }

        protected LineElement(Node node1, Node node2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(new List<int> { node1.Id, node2.Id }, material, crossSection)
        {
            BetaAngle = betaAngle;
        }

        // ID指定コンストラクタ（既存コードとの互換性のため）
        protected LineElement(int id, Point3 point1, Point3 point2, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
            : base(id, new List<Point3> { point1, point2 }, material, crossSection)
        {
            BetaAngle = betaAngle;
        }

        public LineElement(int v1, int v2)
        {
            NodeIds = new List<int> { v1, v2 };
        }

        public LineElement(int id, int node1Id, int node2Id, Material material, CrossSection_Beam crossSection, double betaAngle = 0.0)
        {
            Id = id;
            NodeIds = new List<int> { node1Id, node2Id };
            Material = material;
            CrossSection = crossSection;
            BetaAngle = betaAngle;
        }

        /// <summary>
        /// 線要素の局所座標系を計算（β角方式）
        /// LocalAxisX: 部材軸方向（節点1→節点2）
        /// LocalAxisZ: β角を使用して計算した局所Z軸
        /// LocalAxisY: 右手法則により自動決定（LocalAxisZ × LocalAxisX）
        /// </summary>
        public override void CalcLocalAxis(List<Node> nodes)
        {
            if (NodeIds == null || NodeIds.Count < 2)
                throw new InvalidOperationException($"LineElement(Id={Id}) requires at least 2 nodes.");

            // ノード1とノード2を取得
            int node1Id = NodeIds[0];
            int node2Id = NodeIds[1];
            var node1 = nodes.FirstOrDefault(n => n.Id == node1Id);
            var node2 = nodes.FirstOrDefault(n => n.Id == node2Id);

            if (node1 == null)
                throw new ArgumentException($"LineElement(Id={Id}): Node with Id={node1Id} not found.");
            if (node2 == null)
                throw new ArgumentException($"LineElement(Id={Id}): Node with Id={node2Id} not found.");

            // 部材軸方向ベクトル（X軸）を計算：節点1 → 節点2
            double dx = node2.Position.X - node1.Position.X;
            double dy = node2.Position.Y - node1.Position.Y;
            double dz = node2.Position.Z - node1.Position.Z;
            double L = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (L <= 1e-12)
                throw new ArgumentException($"LineElement(Id={Id}) length is approximately zero. Check node coordinates.");

            // 部材長を設定
            Length = L;

            // 局所X軸（部材軸方向の単位ベクトル）
            double exx = dx / L;
            double exy = dy / L;
            double exz = dz / L;

            // β角をラジアンに変換
            double betaRad = BetaAngle * Math.PI / 180.0;
            double cosBeta = Math.Cos(betaRad);
            double sinBeta = Math.Sin(betaRad);

            // 要素座標系x軸が全体座標系Z軸に平行かチェック
            // |ex · (0,0,1)| ≈ 1 の場合、平行
            double dotWithGlobalZ = exz;
            bool isParallelToGlobalZ = Math.Abs(Math.Abs(dotWithGlobalZ) - 1.0) < 1e-6;

            double ezx, ezy, ezz;

            if (isParallelToGlobalZ)
            {
                // 要素座標系x軸が全体座標系Z軸に平行の場合
                // β角は全体座標系X軸と要素座標系z軸が成す角度
                // 局所Z軸は、全体座標系のXY平面内でX軸からβ角回転した方向
                ezx = cosBeta;
                ezy = sinBeta;
                ezz = 0.0;
            }
            else
            {
                // 要素座標系x軸が全体座標系Z軸に平行でない場合
                // β角は全体座標系Z軸と要素座標系xz平面が成す角度

                // 局所x軸と全体Z軸を含む平面の法線ベクトル n = ex × (0,0,1)
                double nx = exy;   // = exy * 1.0 - exz * 0.0
                double ny = -exx;  // = exz * 0.0 - exx * 1.0
                double nz = 0.0;   // = exx * 0.0 - exy * 0.0
                double nNorm = Math.Sqrt(nx * nx + ny * ny);

                if (nNorm < 1e-12)
                {
                    throw new ArgumentException($"LineElement(Id={Id}) failed to compute local coordinate system.");
                }

                nx /= nNorm;
                ny /= nNorm;

                // 全体Z軸を局所x軸に垂直な平面に射影したベクトル v
                // v = (0,0,1) - ((0,0,1)·ex)ex
                double vx = -dotWithGlobalZ * exx;
                double vy = -dotWithGlobalZ * exy;
                double vz = 1.0 - dotWithGlobalZ * exz;
                double vNorm = Math.Sqrt(vx * vx + vy * vy + vz * vz);

                if (vNorm < 1e-12)
                {
                    throw new ArgumentException($"LineElement(Id={Id}) failed to compute reference vector.");
                }

                vx /= vNorm;
                vy /= vNorm;
                vz /= vNorm;

                // 局所Z軸 = v*cos(β) + n*sin(β)
                ezx = vx * cosBeta + nx * sinBeta;
                ezy = vy * cosBeta + ny * sinBeta;
                ezz = vz * cosBeta + nz * sinBeta;

                // 正規化
                double ezNorm = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);
                ezx /= ezNorm;
                ezy /= ezNorm;
                ezz /= ezNorm;
            }

            // 局所Y軸を外積で計算: ey = ez × ex （右手法則）
            double eyx = ezy * exz - ezz * exy;
            double eyy = ezz * exx - ezx * exz;
            double eyz = ezx * exy - ezy * exx;
            double eyn = Math.Sqrt(eyx * eyx + eyy * eyy + eyz * eyz);

            if (eyn < 1e-12)
                throw new ArgumentException($"LineElement(Id={Id}) failed to compute local Y axis.");

            // 局所Y軸の正規化
            eyx /= eyn;
            eyy /= eyn;
            eyz /= eyn;

            // 局所座標系の基底ベクトルを保存
            LocalAxisX = new double[] { exx, exy, exz };
            LocalAxisY = new double[] { eyx, eyy, eyz };
            LocalAxisZ = new double[] { ezx, ezy, ezz };
        }

        // 座標変換行列: v_g = T v_l（T は diag(R,R,R,R)）
        internal override Matrix<double> CalcTransformationMatrix(List<Node> nodes)
        {
            // CalcLocalAxisで計算済みの基底ベクトルを取得
            double exx = LocalAxisX[0], exy = LocalAxisX[1], exz = LocalAxisX[2];
            double eyx = LocalAxisY[0], eyy = LocalAxisY[1], eyz = LocalAxisY[2];
            double ezx = LocalAxisZ[0], ezy = LocalAxisZ[1], ezz = LocalAxisZ[2];

            // ローカル→グローバル変換行列 R^T（列に ex, ey, ez を配置）
            var R = Matrix<double>.Build.Dense(3, 3);
            R[0, 0] = exx; R[0, 1] = eyx; R[0, 2] = ezx;  // 列0: ex
            R[1, 0] = exy; R[1, 1] = eyy; R[1, 2] = ezy;  // 列1: ey
            R[2, 0] = exz; R[2, 1] = eyz; R[2, 2] = ezz;  // 列2: ez

            // 12x12 のブロック対角 T を明示ループで構成
            var T = Matrix<double>.Build.Dense(12, 12);
            int[] bases = new[] { 0, 3, 6, 9 }; // (0,0), (3,3), (6,6), (9,9)
            foreach (var b in bases)
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        T[b + i, b + j] = R[i, j];
            }

            this.TransformationMatrix = T;
            return T;
        }

        public LineElement(SerializationInfo info, StreamingContext context)
        :base(info, context)
        {
            try
            {
                BetaAngle = info.GetDouble("BetaAngle");
            }
            catch
            {
                BetaAngle = 0.0;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BetaAngle", BetaAngle);
        }
    }
}
