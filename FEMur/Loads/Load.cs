using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;
using System.Runtime.Serialization;

namespace FEMur.Loads
{
    public abstract class Load : CommonObject, ICloneable, ISerializable
    {
        protected string LoadCase { get; set; }
        protected Load() { }
        protected Load(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            LoadCase = info.GetString("LoadCase");
        }
        protected Load(Load other)
        {
            this.LoadCase = other.LoadCase;
        }
        protected Load(string loadCase)
        {
            this.LoadCase = loadCase;
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", LoadCase);
        }
        public override string ToString()
        {
            return $"Load Case: {LoadCase}";
        }

    }
}
