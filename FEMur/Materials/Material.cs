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
        public int Id { get; set; }
        public string Family { get; protected set; } = "Steel";
        public string Name { get; protected set; } = "SS400";
        public double E { get; protected set; } = 20500.0;
        public double Nu { get; protected set; } = 0.3;

        public double G
        {
            get
            {
                return E / (2 * (1 + Nu));
            }
        }

        public double Density { get; protected set; }
        
        protected Material(){}
        protected Material(int id ,string family,string name, double E, double nu, double density)
        {
            this.Id = id;
            this.Family = family;
            this.Name = name;
            this.E = E;
            this.Nu = nu;
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

        //鉄を返す静的メソッド
        public static Material_Isotropic Steel()
        {
            // 単位は N/mm^2, 密度は未使用なら適当でOK
            return new Material_Isotropic(
                id: 0,
                family: "Isotropic",
                name: "Steel",
                E: 210000.0,   // 210 GPa = 210000 N/mm^2
                nu: 0.3,
                density: 7.85e-9 // t/mm^3 等。未使用なら 0 でも可
            );
        }
    }
}
