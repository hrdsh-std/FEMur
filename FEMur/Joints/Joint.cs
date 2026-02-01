using FEMur.Utilities;
using FEMur.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Joints
{
    /// <summary>
    /// 要素の両端部の接合条件を定義するクラス
    /// 特定の要素（ElementId）に対して、Start端（i端）とEnd端（j端）それぞれの回転ばね、並進ばねの剛性を設定可能
    /// </summary>
    public class Joint : CommonObject, ICloneable, IEquatable<Joint>, ISerializable
    {
        public const int DOFS_PER_END = 6; // 各端部の自由度数（DX, DY, DZ, RX, RY, RZ）
        public const int TOTAL_DOFS = 12; // 全体の自由度数（Start端6 + End端6）

        #region Properties

        /// <summary>
        /// 接合条件を適用する要素のID
        /// ElementオブジェクトからElementIdが自動設定される場合もある
        /// </summary>
        public int ElementId { get; set; }

        /// <summary>
        /// 参照する要素オブジェクト（オプショナル）
        /// Grasshopperから要素を直接渡す場合に使用
        /// Model構築時にElementIdが自動設定される
        /// </summary>
        [NonSerialized]
        private LineElement _element;

        /// <summary>
        /// 参照する要素オブジェクト
        /// 設定時にElementIdも自動更新される
        /// </summary>
        public LineElement Element
        {
            get => _element;
            set
            {
                _element = value;
                if (_element != null)
                {
                    ElementId = _element.Id;
                }
            }
        }

        /// <summary>
        /// 接合部の名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Start端（i端）の並進ばね剛性 [N/m]
        /// Index: 0=DX, 1=DY, 2=DZ
        /// </summary>
        public double[] StartTranslationalStiffness { get; private set; }

        /// <summary>
        /// Start端（i端）の回転ばね剛性 [N·m/rad]
        /// Index: 0=RX, 1=RY, 2=RZ
        /// </summary>
        public double[] StartRotationalStiffness { get; private set; }

        /// <summary>
        /// End端（j端）の並進ばね剛性 [N/m]
        /// Index: 0=DX, 1=DY, 2=DZ
        /// </summary>
        public double[] EndTranslationalStiffness { get; private set; }

        /// <summary>
        /// End端（j端）の回転ばね剛性 [N·m/rad]
        /// Index: 0=RX, 1=RY, 2=RZ
        /// </summary>
        public double[] EndRotationalStiffness { get; private set; }

        /// <summary>
        /// 各自由度の拘束フラグ
        /// true: 完全拘束（剛結）, false: ばね or 自由
        /// Index: 0-5: Start端（DX_i, DY_i, DZ_i, RX_i, RY_i, RZ_i）
        /// Index: 6-11: End端（DX_j, DY_j, DZ_j, RX_j, RY_j, RZ_j）
        /// </summary>
        public bool[] IsFixed { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// デフォルトコンストラクタ（両端とも剛結）
        /// </summary>
        public Joint()
        {
            ElementId = -1;
            Name = "Rigid-Rigid";
            StartTranslationalStiffness = new double[3];
            StartRotationalStiffness = new double[3];
            EndTranslationalStiffness = new double[3];
            EndRotationalStiffness = new double[3];
            IsFixed = new bool[TOTAL_DOFS];
            
            // デフォルトは全自由度を剛結
            for (int i = 0; i < TOTAL_DOFS; i++)
            {
                IsFixed[i] = true;
            }
        }

        /// <summary>
        /// 要素オブジェクトを指定したコンストラクタ（Grasshopper用）
        /// ElementIdは要素から自動取得される
        /// </summary>
        /// <param name="element">LineElement（BeamElement, TrussElementなど）</param>
        /// <param name="name">接合部の名前</param>
        public Joint(LineElement element, string name = "Rigid-Rigid") : this()
        {
            Element = element; // ElementIdも自動設定される
            Name = name;
        }

        /// <summary>
        /// 要素IDと名前を指定したコンストラクタ
        /// </summary>
        /// <param name="elementId">要素ID</param>
        /// <param name="name">接合部の名前</param>
        public Joint(int elementId, string name = "Rigid-Rigid") : this()
        {
            ElementId = elementId;
            Name = name;
        }

        /// <summary>
        /// 名前を指定したコンストラクタ（ElementIdは未設定）
        /// 名前だけだと両端とも剛結接合になる
        /// </summary>
        public Joint(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        public Joint(Joint other)
        {
            ElementId = other.ElementId;
            _element = other._element; // 参照をコピー
            Name = other.Name;
            StartTranslationalStiffness = (double[])other.StartTranslationalStiffness.Clone();
            StartRotationalStiffness = (double[])other.StartRotationalStiffness.Clone();
            EndTranslationalStiffness = (double[])other.EndTranslationalStiffness.Clone();
            EndRotationalStiffness = (double[])other.EndRotationalStiffness.Clone();
            IsFixed = (bool[])other.IsFixed.Clone();
        }

        /// <summary>
        /// シリアライゼーション用コンストラクタ
        /// </summary>
        protected Joint(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // 互換性のため、ElementIdがない古いデータにも対応
            try
            {
                ElementId = info.GetInt32("ElementId");
            }
            catch
            {
                ElementId = -1;
            }

            Name = info.GetString("Name");
            StartTranslationalStiffness = (double[])info.GetValue("StartTranslationalStiffness", typeof(double[]));
            StartRotationalStiffness = (double[])info.GetValue("StartRotationalStiffness", typeof(double[]));
            EndTranslationalStiffness = (double[])info.GetValue("EndTranslationalStiffness", typeof(double[]));
            EndRotationalStiffness = (double[])info.GetValue("EndRotationalStiffness", typeof(double[]));
            IsFixed = (bool[])info.GetValue("IsFixed", typeof(bool[]));
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// 両端とも剛結接合を作成（要素オブジェクト指定）
        /// </summary>
        /// <param name="element">LineElement</param>
        public static Joint CreateRigid(LineElement element)
        {
            return new Joint(element, "Rigid-Rigid");
        }

        /// <summary>
        /// 両端とも剛結接合を作成（要素ID指定）
        /// </summary>
        /// <param name="elementId">要素ID</param>
        public static Joint CreateRigid(int elementId)
        {
            return new Joint(elementId, "Rigid-Rigid");
        }

        /// <summary>
        /// 両端ともピン接合を作成（要素オブジェクト指定）
        /// </summary>
        /// <param name="element">LineElement</param>
        public static Joint CreatePin(LineElement element)
        {
            var joint = new Joint(element, "Pin-Pin");
            
            // Start端：並進固定、回転自由
            joint.IsFixed[0] = true;  joint.IsFixed[1] = true;  joint.IsFixed[2] = true;
            joint.IsFixed[3] = false; joint.IsFixed[4] = false; joint.IsFixed[5] = false;
            
            // End端：並進固定、回転自由
            joint.IsFixed[6] = true;  joint.IsFixed[7] = true;  joint.IsFixed[8] = true;
            joint.IsFixed[9] = false; joint.IsFixed[10] = false; joint.IsFixed[11] = false;
            
            return joint;
        }

        /// <summary>
        /// 両端ともピン接合を作成（要素ID指定）
        /// </summary>
        /// <param name="elementId">要素ID</param>
        public static Joint CreatePin(int elementId)
        {
            var joint = new Joint(elementId, "Pin-Pin");
            
            joint.IsFixed[0] = true;  joint.IsFixed[1] = true;  joint.IsFixed[2] = true;
            joint.IsFixed[3] = false; joint.IsFixed[4] = false; joint.IsFixed[5] = false;
            
            joint.IsFixed[6] = true;  joint.IsFixed[7] = true;  joint.IsFixed[8] = true;
            joint.IsFixed[9] = false; joint.IsFixed[10] = false; joint.IsFixed[11] = false;
            
            return joint;
        }

        /// <summary>
        /// Start端をピン、End端を剛結とする接合を作成（要素オブジェクト指定）
        /// </summary>
        public static Joint CreatePinRigid(LineElement element)
        {
            var joint = new Joint(element, "Pin-Rigid");
            
            joint.IsFixed[0] = true;  joint.IsFixed[1] = true;  joint.IsFixed[2] = true;
            joint.IsFixed[3] = false; joint.IsFixed[4] = false; joint.IsFixed[5] = false;
            
            return joint;
        }

        /// <summary>
        /// Start端をピン、End端を剛結とする接合を作成（要素ID指定）
        /// </summary>
        public static Joint CreatePinRigid(int elementId)
        {
            var joint = new Joint(elementId, "Pin-Rigid");
            
            joint.IsFixed[0] = true;  joint.IsFixed[1] = true;  joint.IsFixed[2] = true;
            joint.IsFixed[3] = false; joint.IsFixed[4] = false; joint.IsFixed[5] = false;
            
            return joint;
        }

        /// <summary>
        /// Start端を剛結、End端をピンとする接合を作成（要素オブジェクト指定）
        /// </summary>
        public static Joint CreateRigidPin(LineElement element)
        {
            var joint = new Joint(element, "Rigid-Pin");
            
            joint.IsFixed[6] = true;  joint.IsFixed[7] = true;  joint.IsFixed[8] = true;
            joint.IsFixed[9] = false; joint.IsFixed[10] = false; joint.IsFixed[11] = false;
            
            return joint;
        }

        /// <summary>
        /// Start端を剛結、End端をピンとする接合を作成（要素ID指定）
        /// </summary>
        public static Joint CreateRigidPin(int elementId)
        {
            var joint = new Joint(elementId, "Rigid-Pin");
            
            joint.IsFixed[6] = true;  joint.IsFixed[7] = true;  joint.IsFixed[8] = true;
            joint.IsFixed[9] = false; joint.IsFixed[10] = false; joint.IsFixed[11] = false;
            
            return joint;
        }

        /// <summary>
        /// 半剛接合を作成（要素オブジェクト指定）
        /// </summary>
        public static Joint CreateSemiRigid(
            LineElement element,
            double startRx, double startRy, double startRz,
            double endRx, double endRy, double endRz)
        {
            var joint = new Joint(element, "Semi-Rigid");
            
            joint.IsFixed[0] = true; joint.IsFixed[1] = true; joint.IsFixed[2] = true;
            joint.IsFixed[3] = false; joint.IsFixed[4] = false; joint.IsFixed[5] = false;
            joint.StartRotationalStiffness[0] = startRx;
            joint.StartRotationalStiffness[1] = startRy;
            joint.StartRotationalStiffness[2] = startRz;
            
            joint.IsFixed[6] = true; joint.IsFixed[7] = true; joint.IsFixed[8] = true;
            joint.IsFixed[9] = false; joint.IsFixed[10] = false; joint.IsFixed[11] = false;
            joint.EndRotationalStiffness[0] = endRx;
            joint.EndRotationalStiffness[1] = endRy;
            joint.EndRotationalStiffness[2] = endRz;
            
            return joint;
        }

        /// <summary>
        /// 半剛接合を作成（要素ID指定）
        /// </summary>
        public static Joint CreateSemiRigid(
            int elementId,
            double startRx, double startRy, double startRz,
            double endRx, double endRy, double endRz)
        {
            var joint = new Joint(elementId, "Semi-Rigid");
            
            joint.IsFixed[0] = true; joint.IsFixed[1] = true; joint.IsFixed[2] = true;
            joint.IsFixed[3] = false; joint.IsFixed[4] = false; joint.IsFixed[5] = false;
            joint.StartRotationalStiffness[0] = startRx;
            joint.StartRotationalStiffness[1] = startRy;
            joint.StartRotationalStiffness[2] = startRz;
            
            joint.IsFixed[6] = true; joint.IsFixed[7] = true; joint.IsFixed[8] = true;
            joint.IsFixed[9] = false; joint.IsFixed[10] = false; joint.IsFixed[11] = false;
            joint.EndRotationalStiffness[0] = endRx;
            joint.EndRotationalStiffness[1] = endRy;
            joint.EndRotationalStiffness[2] = endRz;
            
            return joint;
        }

        #endregion

        #region Public Methods - Start端

        /// <summary>
        /// Start端の並進ばね剛性を設定
        /// </summary>
        public void SetStartTranslationalStiffness(double dx, double dy, double dz)
        {
            StartTranslationalStiffness[0] = dx;
            StartTranslationalStiffness[1] = dy;
            StartTranslationalStiffness[2] = dz;
        }

        /// <summary>
        /// Start端の回転ばね剛性を設定
        /// </summary>
        public void SetStartRotationalStiffness(double rx, double ry, double rz)
        {
            StartRotationalStiffness[0] = rx;
            StartRotationalStiffness[1] = ry;
            StartRotationalStiffness[2] = rz;
        }

        /// <summary>
        /// Start端の指定された自由度を固定
        /// </summary>
        /// <param name="localDofIndex">ローカル自由度インデックス (0-5)</param>
        public void FixStartDOF(int localDofIndex)
        {
            if (localDofIndex >= 0 && localDofIndex < DOFS_PER_END)
            {
                IsFixed[localDofIndex] = true;
            }
        }

        /// <summary>
        /// Start端の指定された自由度を解放
        /// </summary>
        public void ReleaseStartDOF(int localDofIndex)
        {
            if (localDofIndex >= 0 && localDofIndex < DOFS_PER_END)
            {
                IsFixed[localDofIndex] = false;
            }
        }

        #endregion

        #region Public Methods - End端

        /// <summary>
        /// End端の並進ばね剛性を設定
        /// </summary>
        public void SetEndTranslationalStiffness(double dx, double dy, double dz)
        {
            EndTranslationalStiffness[0] = dx;
            EndTranslationalStiffness[1] = dy;
            EndTranslationalStiffness[2] = dz;
        }

        /// <summary>
        /// End端の回転ばね剛性を設定
        /// </summary>
        public void SetEndRotationalStiffness(double rx, double ry, double rz)
        {
            EndRotationalStiffness[0] = rx;
            EndRotationalStiffness[1] = ry;
            EndRotationalStiffness[2] = rz;
        }

        /// <summary>
        /// End端の指定された自由度を固定
        /// </summary>
        /// <param name="localDofIndex">ローカル自由度インデックス (0-5)</param>
        public void FixEndDOF(int localDofIndex)
        {
            if (localDofIndex >= 0 && localDofIndex < DOFS_PER_END)
            {
                IsFixed[DOFS_PER_END + localDofIndex] = true;
            }
        }

        /// <summary>
        /// End端の指定された自由度を解放
        /// </summary>
        public void ReleaseEndDOF(int localDofIndex)
        {
            if (localDofIndex >= 0 && localDofIndex < DOFS_PER_END)
            {
                IsFixed[DOFS_PER_END + localDofIndex] = false;
            }
        }

        #endregion

        #region Public Methods - 取得

        /// <summary>
        /// 指定された端部の回転ばね剛性を取得
        /// </summary>
        /// <param name="isStartEnd">true: Start端, false: End端</param>
        /// <param name="rotationIndex">回転方向インデックス (0-2: RX, RY, RZ)</param>
        /// <returns>ばね剛性 [N·m/rad]、固定の場合は無限大</returns>
        public double GetRotationalStiffness(bool isStartEnd, int rotationIndex)
        {
            if (rotationIndex < 0 || rotationIndex >= 3)
                throw new ArgumentException("Rotation index must be 0, 1, or 2 (RX, RY, RZ)");

            int dofIndex = (isStartEnd ? 0 : DOFS_PER_END) + 3 + rotationIndex;

            if (IsFixed[dofIndex])
                return double.PositiveInfinity;

            return isStartEnd ? StartRotationalStiffness[rotationIndex] : EndRotationalStiffness[rotationIndex];
        }

        /// <summary>
        /// Start端が剛結かどうかを判定
        /// </summary>
        public bool IsStartRigid()
        {
            for (int i = 0; i < DOFS_PER_END; i++)
            {
                if (!IsFixed[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// End端が剛結かどうかを判定
        /// </summary>
        public bool IsEndRigid()
        {
            for (int i = DOFS_PER_END; i < TOTAL_DOFS; i++)
            {
                if (!IsFixed[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Start端がピンかどうかを判定
        /// </summary>
        public bool IsStartPin()
        {
            return IsFixed[0] && IsFixed[1] && IsFixed[2] &&      // 並進固定
                   !IsFixed[3] && !IsFixed[4] && !IsFixed[5];    // 回転自由
        }

        /// <summary>
        /// End端がピンかどうかを判定
        /// </summary>
        public bool IsEndPin()
        {
            return IsFixed[6] && IsFixed[7] && IsFixed[8] &&      // 並進固定
                   !IsFixed[9] && !IsFixed[10] && !IsFixed[11];   // 回転自由
        }

        /// <summary>
        /// 要素オブジェクトから ElementId を更新
        /// Model構築時に呼び出される
        /// </summary>
        internal void UpdateElementIdFromElement()
        {
            if (_element != null)
            {
                ElementId = _element.Id;
            }
        }

        #endregion

        #region CommonObject Override

        public override object DeepCopy()
        {
            return new Joint(this);
        }

        public override string ToString()
        {
            string startType = IsStartRigid() ? "Rigid" : (IsStartPin() ? "Pin" : "Semi-Rigid");
            string endType = IsEndRigid() ? "Rigid" : (IsEndPin() ? "Pin" : "Semi-Rigid");
            
            string elementInfo = _element != null ? $"Element (ID will be assigned)" : $"Element {ElementId}";
            return $"Joint: {elementInfo} - {Name} (Start: {startType}, End: {endType})";
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(Joint other)
        {
            if (other == null) return false;
            
            return ElementId == other.ElementId &&
                   Name == other.Name &&
                   StartTranslationalStiffness.SequenceEqual(other.StartTranslationalStiffness) &&
                   StartRotationalStiffness.SequenceEqual(other.StartRotationalStiffness) &&
                   EndTranslationalStiffness.SequenceEqual(other.EndTranslationalStiffness) &&
                   EndRotationalStiffness.SequenceEqual(other.EndRotationalStiffness) &&
                   IsFixed.SequenceEqual(other.IsFixed);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Joint);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ElementId.GetHashCode();
                hash = hash * 31 + (Name?.GetHashCode() ?? 0);
                foreach (var s in StartRotationalStiffness)
                    hash = hash * 31 + s.GetHashCode();
                foreach (var s in EndRotationalStiffness)
                    hash = hash * 31 + s.GetHashCode();
                return hash;
            }
        }

        #endregion

        #region ISerializable Implementation

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ElementId", ElementId);
            info.AddValue("Name", Name);
            info.AddValue("StartTranslationalStiffness", StartTranslationalStiffness);
            info.AddValue("StartRotationalStiffness", StartRotationalStiffness);
            info.AddValue("EndTranslationalStiffness", EndTranslationalStiffness);
            info.AddValue("EndRotationalStiffness", EndRotationalStiffness);
            info.AddValue("IsFixed", IsFixed);
        }

        #endregion
    }
}
