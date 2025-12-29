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
        public Material_Isotropic(int  id,string family, string name, double E, double nu, double density)
            : base(id,family, name, E, E / (2 * (1 + nu)), density)
        {
            this.E = E;
            this.nu = nu;
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
