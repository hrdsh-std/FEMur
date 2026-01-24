using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.CrossSections
{
    public class CrossSection_Shell : CrossSection,ISerializable
    {
        public double Thickness { get; }

        public CrossSection_Shell()
        {
        }

        public CrossSection_Shell(double thickness)
        {
            Thickness = thickness;
        }

        public CrossSection_Shell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { 
        }



    }
}
