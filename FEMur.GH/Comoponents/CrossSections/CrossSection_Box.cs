using System;
using Grasshopper.Kernel;
using FEMur.CrossSections;

namespace FEMurGH.Comoponents.CrossSections
{
    /// <summary>
    /// 角形鋼管断面の CrossSection を生成するコンポーネント。
    /// 入力: Name, B, H, t, r
    /// 出力: CrossSection_Beam（実体は CrossSection_Box）
    /// </summary>
    public class CrossSection_Box : GH_Component
    {
        public CrossSection_Box()
          : base("CrossSection Box (FEMur)", "XSec-Box",
              "Create Box section (CrossSection_Box) for beam elements.",
              "FEMur", "2.CrossSection")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Name", "Name", "CrossSection name (optional, auto-generated if empty)", GH_ParamAccess.item, string.Empty);

            p.AddNumberParameter("B", "B", "Width B [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("H", "H", "Height H [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("t", "t", "Thickness t [mm] (0 for solid rectangle)", GH_ParamAccess.item, 0.0);
            p.AddNumberParameter("r", "r", "Corner radius r [mm] (optional, default=1.5*t for hollow, ignored for solid)", GH_ParamAccess.item, -1.0);

            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam (CrossSection_Box)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            double B = 100.0, H = 100.0, t = 0.0, r = -1.0;

            DA.GetData(0, ref name);
            DA.GetData(1, ref B);
            DA.GetData(2, ref H);
            DA.GetData(3, ref t);
            DA.GetData(4, ref r);

            // Name が空の場合は自動生成
            if (string.IsNullOrWhiteSpace(name))
            {
                if (t > 0)
                {
                    // 中空: □-BxHxt
                    name = $"□-{FormatDimension(B)}x{FormatDimension(H)}x{FormatDimension(t)}";
                }
                else
                {
                    // 中実: □-BxH
                    name = $"□-{FormatDimension(B)}x{FormatDimension(H)}";
                }
            }

            // CrossSection_Box を作成
            CrossSection_Beam xsec = new FEMur.CrossSections.CrossSection_Box(name, B, H, t, r);

            DA.SetData(0, xsec);
        }

        private string FormatDimension(double value)
        {
            if (Math.Abs(value - Math.Round(value)) < 1e-10)
            {
                return value.ToString("F0");
            }
            else
            {
                return value.ToString("G");
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("8B3A4F2E-1C5D-4E9A-B7F3-2D6E8A9C1F4B");
    }
}