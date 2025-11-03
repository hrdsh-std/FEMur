using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Geometry;

namespace FEMur.Loads
{
    public abstract class PointAction : Load, ISerializable
    {
        protected Vector3 ActionTranslation { get; set; }
        protected Vector3 ActionRotation { get; set; }
        public int NodeId { get; set; }

        public PointAction() { }
        public PointAction(PointAction other)
            : base(other)
        {
            this.ActionTranslation = other.ActionTranslation;
            this.ActionRotation = other.ActionRotation;
            this.NodeId = other.NodeId;
        }

        public PointAction(int idx, Vector3 translation, Vector3 rotation)
        {
            this.NodeId = idx;
            this.ActionTranslation = translation;
            this.ActionRotation = rotation;
        }
        protected PointAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ActionTranslation = (Vector3)info.GetValue("ActionTranslation", typeof(Vector3));
            ActionRotation = (Vector3)info.GetValue("ActionRotation", typeof(Vector3));
            NodeId = info.GetInt32("NodeId");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ActionTranslation", ActionTranslation);
            info.AddValue("ActionRotation", ActionRotation);
            info.AddValue("NodeId", NodeId);
        }
    }
}
