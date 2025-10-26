using FEMur.CrossSections;
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
    public abstract class LineElement:ElementBase,ISerializable
    {
        public LineElement() { }
        
        LineElement(int id,List<int> nodeIds, Material material, CrossSection_Beam crossSection)
            :base(id,nodeIds,material,crossSection)
        {

        }
        LineElement(int id, List<Node> nodes, Material material, CrossSection_Beam crossSection)
            : base(id, nodes.Select(n => n.Id).ToList(), material, crossSection)
        {
        }
        LineElement(int id, Node node1, Node node2, Material material, CrossSection_Beam crossSection)
            : base(id, new List<int> { node1.Id, node2.Id }, material, crossSection)
        {
        }


        public LineElement(int v1, int v2) 
        {
            NodeIds = new List<int> { v1, v2 };
        }
        public LineElement(int id, int node1Id, int node2Id, Material material, CrossSection_Beam crossSection)
        {
            Id = id;
            NodeIds = new List<int> { node1Id,node2Id };
            Material = material;
            CrossSection = crossSection;
        }
        
        /// <summary>
        /// 線要素の局所座標系を計算
        /// LocalAxisX: 部材軸方向（始点→終点）
        /// LocalAxisZ: LocalAxisプロパティから計算した局所Z軸
        /// LocalAxisY: Z軸×X軸の外積
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

            // 部材軸方向ベクトル（X軸）を計算
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

            // 参照ベクトル（LocalAxisプロパティ、デフォルトは{0,0,1}）
            double[] vrefArr = LocalAxis ?? new double[3] { 0, 0, 1 };
            double vx = vrefArr[0];
            double vy = vrefArr[1];
            double vz = vrefArr[2];

            // 局所Z軸を計算（参照ベクトルから部材軸方向成分を除去）
            double dot = vx * exx + vy * exy + vz * exz;
            double ezx = vx - dot * exx;
            double ezy = vy - dot * exy;
            double ezz = vz - dot * exz;
            double ezn = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);

            // 参照ベクトルが部材軸と平行な場合、別の参照ベクトルを試す
            if (ezn < 1e-12)
            {
                // Y軸方向を試す
                vx = 0; vy = 1; vz = 0;
                dot = vx * exx + vy * exy + vz * exz;
                ezx = vx - dot * exx;
                ezy = vy - dot * exy;
                ezz = vz - dot * exz;
                ezn = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);

                if (ezn < 1e-12)
                {
                    // X軸方向を試す
                    vx = 1; vy = 0; vz = 0;
                    dot = vx * exx + vy * exy + vz * exz;
                    ezx = vx - dot * exx;
                    ezy = vy - dot * exy;
                    ezz = vz - dot * exz;
                    ezn = Math.Sqrt(ezx * ezx + ezy * ezy + ezz * ezz);

                    if (ezn < 1e-12)
                        throw new ArgumentException($"LineElement(Id={Id}) cannot build local axes (all reference vectors parallel to element).");
                }
            }

            // 局所Z軸の正規化
            ezx /= ezn;
            ezy /= ezn;
            ezz /= ezn;

            // 局所Y軸を外積で計算: ey = ez × ex
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
            // 局所座標系が未計算の場合は計算
            if (LocalAxisX == null || LocalAxisY == null || LocalAxisZ == null || Length <= 0)
            {
                CalcLocalAxis(nodes);
            }

            // CalcLocalAxisで計算済みの基底ベクトルを取得
            double exx = LocalAxisX[0], exy = LocalAxisX[1], exz = LocalAxisX[2];
            double eyx = LocalAxisY[0], eyy = LocalAxisY[1], eyz = LocalAxisY[2];
            double ezx = LocalAxisZ[0], ezy = LocalAxisZ[1], ezz = LocalAxisZ[2];

            // グローバル→ローカル変換行列 R（行に ex, ey, ez を配置）
            var R = Matrix<double>.Build.Dense(3, 3);
            R[0, 0] = exx; R[0, 1] = exy; R[0, 2] = exz;  // 行0: ex
            R[1, 0] = eyx; R[1, 1] = eyy; R[1, 2] = eyz;  // 行1: ey
            R[2, 0] = ezx; R[2, 1] = ezy; R[2, 2] = ezz;  // 行2: ez

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
            throw new NotImplementedException();
        }
    }
}
