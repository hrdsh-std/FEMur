using System;
using Grasshopper.Kernel;
using FEMur.Materials;

namespace FEMurGH.Comoponents.Materials
{
    /// <summary>
    /// Material_Orthotropic コンポーネント
    /// - 直交異方性材料を作成
    /// - すべてのパラメータを入力で指定
    /// </summary>
    public class Material_Orthotropic : GH_Component
    {
        public Material_Orthotropic()
          : base(
              "Material Orthotropic (FEMur)",
              "Mat-Ortho",
              "Create orthotropic material with directional properties.",
              "FEMur",
              "3.Material")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Name", "Name", "Material name", GH_ParamAccess.item, "Orthotropic");
            p.AddNumberParameter("Density", "Rho", "Density [t/mm^3]", GH_ParamAccess.item, 1.0e-9);

            // 弾性係数
            p.AddNumberParameter("Ex", "Ex", "Young's modulus in X direction [N/mm^2]", GH_ParamAccess.item, 10000.0);
            p.AddNumberParameter("Ey", "Ey", "Young's modulus in Y direction [N/mm^2]", GH_ParamAccess.item, 1000.0);
            p.AddNumberParameter("Ez", "Ez", "Young's modulus in Z direction [N/mm^2]", GH_ParamAccess.item, 1000.0);

            // せん断弾性係数
            p.AddNumberParameter("Gxy", "Gxy", "Shear modulus in XY plane [N/mm^2]", GH_ParamAccess.item, 500.0);
            p.AddNumberParameter("Gyz", "Gyz", "Shear modulus in YZ plane [N/mm^2]", GH_ParamAccess.item, 300.0);
            p.AddNumberParameter("Gzx", "Gzx", "Shear modulus in ZX plane [N/mm^2]", GH_ParamAccess.item, 300.0);

            // ポアソン比
            p.AddNumberParameter("nu_xy", "nu_xy", "Poisson's ratio XY", GH_ParamAccess.item, 0.3);
            p.AddNumberParameter("nu_yz", "nu_yz", "Poisson's ratio YZ", GH_ParamAccess.item, 0.3);
            p.AddNumberParameter("nu_zx", "nu_zx", "Poisson's ratio ZX", GH_ParamAccess.item, 0.3);

            // すべて任意入力
            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("Material", "Mat", "FEMur Material (orthotropic)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "Orthotropic";
            double density = 1.0e-9;
            double Ex = 10000.0, Ey = 1000.0, Ez = 1000.0;
            double Gxy = 500.0, Gyz = 300.0, Gzx = 300.0;
            double nuxy = 0.3, nuyz = 0.3, nuzx = 0.3;

            DA.GetData(0, ref name);
            DA.GetData(1, ref density);
            DA.GetData(2, ref Ex);
            DA.GetData(3, ref Ey);
            DA.GetData(4, ref Ez);
            DA.GetData(5, ref Gxy);
            DA.GetData(6, ref Gyz);
            DA.GetData(7, ref Gzx);
            DA.GetData(8, ref nuxy);
            DA.GetData(9, ref nuyz);
            DA.GetData(10, ref nuzx);

            // Material_Orthotropic を作成（Idなし）
            var mat = new FEMur.Materials.Material_Orthotropic(
                family: "Orthotropic",
                name: name ?? "Orthotropic",
                density: density,
                ex: Ex, ey: Ey, ez: Ez,
                gxy: Gxy, gyz: Gyz, gzx: Gzx,
                nuxy: nuxy, nuyz: nuyz, nuzx: nuzx
            );

            DA.SetData(0, mat);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("3A7C9E2F-5B8D-4E6A-9F1C-8D3E5A7B9C2F");
    }
}