using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using FEMur.Geometry;

namespace FEMur.Loads
{
    public class PointLoad : PointAction, ISerializable
    {
        public Vector3 Force
        {
            get => this.ActionTranslation;
            set => this.ActionTranslation = value;
        }

        public Vector3 Moment
        {
            get => this.ActionRotation;
            set => this.ActionRotation = value;
        }

        public PointLoad() { }

        public PointLoad(PointLoad other)
            : base(other)
        {
            this.Force = other.Force;
            this.Moment = other.Moment;
        }

        public PointLoad(int idx, Vector3 force, Vector3 moment, bool local)
            : base(idx, force, moment, local)
        {
        }
        protected PointLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Force = (Vector3)info.GetValue("Force", typeof(Vector3));
            Moment = (Vector3)info.GetValue("Moment", typeof(Vector3));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Force", Force);
            info.AddValue("Moment", Moment);
        }
        public override string ToString()
        {
            return $"Point Load: Force = {Force}, Moment = {Moment}, NodeId = {NodeId}, Local = {Local}";
        }
    }
}
