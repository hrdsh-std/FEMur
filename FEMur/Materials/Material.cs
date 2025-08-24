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
        public string Family { get; protected set; } = "Steel";
        public string Name { get; protected set; } = "SS400";
        public double E { get; protected set; } = 20500.0;
        public double G { get; protected set; } = 8076.0;
        public double Density { get; protected set; }
        
        protected Material(){}
        protected Material(string family,string name, double E, double G, double density)
        {
            this.Family = family;
            this.Name = name;
            this.E = E;
            this.G = G;
            this.Density = density;
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
