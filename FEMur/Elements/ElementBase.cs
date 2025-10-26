using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;
using MathNet.Numerics.LinearAlgebra;
using FEMur.Nodes;
using FEMur.Materials;
using FEMur.CrossSections;


namespace FEMur.Elements
{
    public abstract class ElementBase : CommonObject, ICloneable, ISerializable
    {
        public int Id { get; set; }
        public List<int> NodeIds { get; set; }
        public Material Material { get; set; }
        public CrossSection CrossSection { get; set; }
        internal double Length { get; set; }
        internal double[] LocalAxis { get; set; } = new double[3] { 0, 0, 1 };
        internal Matrix<double> TransformationMatrix { get; set; }
        internal Matrix<double> LocalStiffness { get; set; }
        internal Matrix<double> GlobalStiffness { get; set; }
        internal Matrix<double> LocalMass { get; set; }
        internal Matrix<double> GlobalMass { get; set; }
        internal Matrix<double> LocalDamping { get; set; }
        internal Matrix<double> GlobalDamping { get; set; }

        // 局所座標系の基底ベクトル（グローバル座標系での表現）
        // ex: 部材軸方向、ey: 局所Y軸、ez: 局所Z軸
        public double[] LocalAxisX { get; protected set; } = null;
        public double[] LocalAxisY { get; protected set; } = null;
        public double[] LocalAxisZ { get; protected set; } = null;

        protected ElementBase()
        {
        }
        protected ElementBase(int id, List<int> nodeIds, Material material, CrossSection_Beam crossSection)
        {
            Id = id;
            NodeIds = nodeIds;
            Material = material;
            CrossSection = crossSection;
        }

        protected ElementBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal abstract Matrix<double> CalcLocalStiffness(List<Node> nodes);

        internal abstract Matrix<double> CalcTransformationMatrix(List<Node> nodes);

        public abstract void CalcLocalAxis(List<Node> nodes);

        /// <summary>
        /// 局所座標系の基準ベクトル（ex, ey, ez）を取得
        /// </summary>
        /// <param name="ex">部材軸方向の単位ベクトル（出力）</param>
        /// <param name="ey">局所Y軸の単位ベクトル（出力）</param>
        /// <param name="ez">局所Z軸の単位ベクトル（出力）</param>
        /// <returns>局所座標系が計算済みの場合true</returns>
        public bool TryGetLocalCoordinateSystem(out double[] ex, out double[] ey, out double[] ez)
        {
            ex = LocalAxisX;
            ey = LocalAxisY;
            ez = LocalAxisZ;
            return ex != null && ey != null && ez != null;
        }


        //Tostringの実装を強制
        public abstract override string ToString();
    }
}
