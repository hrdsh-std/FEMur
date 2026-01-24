using System;
using Grasshopper.Kernel;
using FEMur.CrossSections;

namespace FEMurGH.Comoponents.CrossSections
{
    /// <summary>
    /// 円形断面の CrossSection を生成するコンポーネント。
    /// 入力: Name, D, t
    /// 出力: CrossSection_Beam（実体は CrossSection_Circle）
    /// </summary>
    public class CrossSection_Circle : GH_Component
    {
        public CrossSection_Circle()
          : base("CrossSection Circle (FEMur)", "XSec-Circle",
              "Create Circle section (CrossSection_Circle) for beam elements.",
              "FEMur", "2.CrossSection")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Name", "Name", "CrossSection name (optional, auto-generated if empty)", GH_ParamAccess.item, string.Empty);

            p.AddNumberParameter("D", "D", "Outer diameter D [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("t", "t", "Wall thickness t [mm] (0 for solid circle)", GH_ParamAccess.item, 0.0);

            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam (CrossSection_Circle)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            double D = 100.0, t = 0.0;

            DA.GetData(0, ref name);
            DA.GetData(1, ref D);
            DA.GetData(2, ref t);

            // Name が空の場合は自動生成: ○-DxT (中空) または ○-D (中実)
            if (string.IsNullOrWhiteSpace(name))
            {
                if (t > 0)
                {
                    // 中空円形: ○-DxT
                    name = $"○-{D:F0}x{t:F0}";
                }
                else
                {
                    // 中実円形: ○-D
                    name = $"○-{D:F0}";
                }
            }

            // CrossSection_Circle を作成
            CrossSection_Beam xsec = new FEMur.CrossSections.CrossSection_Circle(name, D, t);

            DA.SetData(0, xsec);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("7C4B5E3F-2D6A-4F8B-A9C2-3E7F9B1D5A6C");
    }
}