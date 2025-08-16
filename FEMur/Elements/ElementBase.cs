using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Utilities;

namespace FEMur.Elements
{
    public abstract class ElementBase: CommonObject,ICloneable,ISerializable
    {
        protected ElementBase()
        {
        }

        protected ElementBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
