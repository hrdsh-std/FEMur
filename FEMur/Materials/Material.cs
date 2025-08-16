using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Materials
{
    public abstract class Material:ISerializable
    {
        public string Name { get; protected set; }
        public double Density { get; protected set; }
        
        protected Material(string name, double density)
        {
            Name = name;
            Density = density;
        }
        protected Material(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            Density = info.GetDouble("Density");
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Density", Density);
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        public override string ToString()
        {
            return $"Material: {Name}, Density: {Density} kg/m^3";
        }

    }
}
