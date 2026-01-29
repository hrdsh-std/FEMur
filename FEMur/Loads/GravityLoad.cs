using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace FEMur.Loads
{
    public class GravityLoad : Load, ISerializable
    {
        public GravityLoad()
        {
            throw new NotImplementedException("This class is not implemented yet. Please implement the GravityLoad class according to your requirements.");
        }
        public GravityLoad(GravityLoad other) : base(other)
        {
            throw new NotImplementedException("This class is not implemented yet. Please implement the GravityLoad class according to your requirements.");
        }
        public override object DeepCopy()
        {
            return new GravityLoad(this);
        }
        // Additional properties and methods for GravityLoad can be added here
    }
}
