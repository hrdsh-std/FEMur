using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FEMur.Geometry;

namespace FEMur.Elements
{
    //abstract class for line elements such as beams, trusses, etc.
    public abstract class LineElement:ElementBase,ISerializable
    {
        public List<int> Nodes { get; }
        protected LineElement() { }
        protected LineElement(int v1, int v2)
        {
            Nodes = new List<int> { v1, v2 };
        }
        protected LineElement(SerializationInfo info, StreamingContext context)
        :base(info, context)
        {
            Nodes = (List<int>)info.GetValue("Nodes", typeof(List<int>));
        }
        public void disassembleElement()
        {
            throw new NotImplementedException("Disassembling line elements is not implemented yet.");
        }
    }
}
