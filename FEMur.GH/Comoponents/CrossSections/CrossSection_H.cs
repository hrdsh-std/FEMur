using System;
using Grasshopper.Kernel;
using FEMur.CrossSections;

namespace FEMurGH.Comoponents.CrossSections
{
    /// <summary>
    /// H断面の CrossSection を生成するコンポーネント。
    /// 入力: Name, H, B, t_w, t_f, r
    /// 出力: CrossSection_Beam（実体は CrossSection_H）
    /// </summary>
    public class CrossSection_H : GH_Component
    {
        public CrossSection_H()
          : base("CrossSection H (FEMur)", "XSec-H",
              "Create H-section (CrossSection_H) for beam elements.",
              "FEMur", "2.CrossSection")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Name", "Name", "CrossSection name (optional, auto-generated if empty)", GH_ParamAccess.item, string.Empty);

            p.AddNumberParameter("H", "H", "Overall height H [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("B", "B", "Flange width B [mm]", GH_ParamAccess.item, 200.0);
            p.AddNumberParameter("t_w", "t_w", "Web thickness t_w [mm]", GH_ParamAccess.item, 5.5);
            p.AddNumberParameter("t_f", "t_f", "Flange thickness t_f [mm]", GH_ParamAccess.item, 8.0);
            p.AddNumberParameter("r", "r", "Root radius r [mm]", GH_ParamAccess.item, 8.0);

            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam (CrossSection_H)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            double H = 100.0, B = 200.0, tw = 5.5, tf = 8.0, r = 8.0;

            DA.GetData(0, ref name);
            DA.GetData(1, ref H);
            DA.GetData(2, ref B);
            DA.GetData(3, ref tw);
            DA.GetData(4, ref tf);
            DA.GetData(5, ref r);

            // Name が空の場合は自動生成: H-HxBxtwxtf
            if (string.IsNullOrWhiteSpace(name))
            {
                // 各値を整数かどうかチェックして適切なフォーマットを選択
                string hStr = FormatDimension(H);
                string bStr = FormatDimension(B);
                string twStr = FormatDimension(tw);
                string tfStr = FormatDimension(tf);

                name = $"H-{hStr}x{bStr}x{twStr}x{tfStr}";
            }

            // CrossSection_H を作成（Idなし）
            CrossSection_Beam xsec = new FEMur.CrossSections.CrossSection_H(name, B, H, tf, tw, r);

            DA.SetData(0, xsec);
        }

        /// <summary>
        /// 寸法値をフォーマット（整数の場合は小数点なし、小数の場合は必要な桁数のみ表示）
        /// </summary>
        private string FormatDimension(double value)
        {
            // 整数かどうかをチェック（誤差を考慮）
            if (Math.Abs(value - Math.Round(value)) < 1e-10)
            {
                // 整数の場合は小数点なし
                return value.ToString("F0");
            }
            else
            {
                // 小数の場合は不要な末尾のゼロを削除
                return value.ToString("G");
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("92EC7925-2980-4641-8761-79E699746235");
    }
}