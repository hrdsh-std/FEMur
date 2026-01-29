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
using FEMur.Geometry;


namespace FEMur.Elements
{
    public abstract class ElementBase : CommonObject, ICloneable, ISerializable
    {
        public int Id { get; set; } // setterを追加（自動採番のため）
        public List<int> NodeIds { get; set; }
        public List<Point3> Points { get; set; } = null;
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
        public double[] LocalAxisX { get; set; } = null;
        public double[] LocalAxisY { get; set; } = null;
        public double[] LocalAxisZ { get; set; } = null;

        protected ElementBase()
        {
            Id = -1; // 未割り当て
        }

        // IDなしコンストラクタ（推奨）
        protected ElementBase(List<int> nodeIds, Material material, CrossSection crossSection)
        {
            Id = -1; // 未割り当て
            NodeIds = nodeIds;
            Material = material;
            CrossSection = crossSection;
        }

        protected ElementBase(List<Point3> points, Material material, CrossSection crossSection)
        {
            Id = -1; // 未割り当て
            Points = points;
            Material = material;
            CrossSection = crossSection;
        }

        // ID指定コンストラクタ（既存コードとの互換性のため残す）
        protected ElementBase(int id, List<int> nodeIds, Material material, CrossSection crossSection)
        {
            Id = id;
            NodeIds = nodeIds;
            Material = material;
            CrossSection = crossSection;
        }

        protected ElementBase(int id, List<Point3> points, Material material, CrossSection crossSection)
        {
            Id = id;
            Points = points;
            Material = material;
            CrossSection = crossSection;
        }

        protected ElementBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        /// <summary>
        /// コピーコンストラクタ（シャローコピー）
        /// DeepCopyが必要な場合は派生クラスでDeepCopy()メソッドを使用すること
        /// </summary>
        public ElementBase(ElementBase other)
        {
            // 値型フィールド
            this.Id = other.Id;
            this.Length = other.Length;
            
            // 参照型フィールド（シャローコピー）
            // Material と CrossSection は不変オブジェクトと仮定して参照をコピー
            this.Material = other.Material;
            this.CrossSection = other.CrossSection;
            
            // リストのシャローコピー（新しいリストだが中身は同じ参照）
            this.NodeIds = other.NodeIds != null ? new List<int>(other.NodeIds) : null;
            this.Points = other.Points != null ? new List<Point3>(other.Points) : null;
            
            // 配列のコピー
            this.LocalAxis = other.LocalAxis != null ? (double[])other.LocalAxis.Clone() : null;
            this.LocalAxisX = other.LocalAxisX != null ? (double[])other.LocalAxisX.Clone() : null;
            this.LocalAxisY = other.LocalAxisY != null ? (double[])other.LocalAxisY.Clone() : null;
            this.LocalAxisZ = other.LocalAxisZ != null ? (double[])other.LocalAxisZ.Clone() : null;
            
            // 行列のシャローコピー（MathNet.Numerics.LinearAlgebra.Matrix<double>）
            // Note: Matrix<T>は参照型だが、通常は再計算されるのでシャローコピーで十分
            this.TransformationMatrix = other.TransformationMatrix;
            this.LocalStiffness = other.LocalStiffness;
            this.GlobalStiffness = other.GlobalStiffness;
            this.LocalMass = other.LocalMass;
            this.GlobalMass = other.GlobalMass;
            this.LocalDamping = other.LocalDamping;
            this.GlobalDamping = other.GlobalDamping;
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
