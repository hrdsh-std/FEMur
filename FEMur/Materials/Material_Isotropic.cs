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
        public double E { get; set; }
        public double nu { get; set; }
        
        public Material_Isotropic()
        {
        }
        //シリアライズ用コンストラクタ
        public Material_Isotropic(SerializationInfo info, StreamingContext context):base(info, context)
        {
            E = info.GetDouble("YoungsModulus");
            nu = info.GetDouble("PoissonsRatio");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("YoungsModulus", E);
            info.AddValue("PoissonsRatio", nu);
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        public override string ToString()
        {
            return $"{base.ToString()}, Young's Modulus: {E} Pa, Poisson's Ratio: {nu}";
        }
    }
}
