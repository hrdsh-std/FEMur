using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Materials
{
    public class Material_Isotropic : Material,ISerializable
    {
        public double YoungsModulus { get; set; }
        public double PoissonsRatio { get; set; }
        
        public Material_Isotropic(
            string name,
            double density,
            double youngsModulus,
            double poissonsRatio
            ):base(name, density)
        {
            YoungsModulus = youngsModulus;
            PoissonsRatio = poissonsRatio;
        }
        //シリアライズ用コンストラクタ
        public Material_Isotropic(SerializationInfo info, StreamingContext context):base(info, context)
        {
            YoungsModulus = info.GetDouble("YoungsModulus");
            PoissonsRatio = info.GetDouble("PoissonsRatio");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("YoungsModulus", YoungsModulus);
            info.AddValue("PoissonsRatio", PoissonsRatio);
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        public override string ToString()
        {
            return $"{base.ToString()}, Young's Modulus: {YoungsModulus} Pa, Poisson's Ratio: {PoissonsRatio}";
        }
    }
}
