using System;
using Grasshopper.Kernel;
using FEMur.CrossSections;

namespace FEMurGH.CrossSections
{
    /// <summary>
    /// H断面の CrossSection を生成するコンポーネント。
    /// 入力: Id, Name, B, H, t_f, t_w, r
    /// 出力: CrossSection_Beam（実体は CrossSection_H）
    /// </summary>
    public class CrossSection_H : GH_Component
    {
        public CrossSection_H()
          : base("CrossSection H (FEMur)", "XSec-H",
              "Create H-section (CrossSection_H) for beam elements.",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddIntegerParameter("Id", "Id", "CrossSection Id", GH_ParamAccess.item, 0);
            p.AddTextParameter("Name", "Name", "CrossSection name", GH_ParamAccess.item, "H-Section");

            p.AddNumberParameter("B", "B", "Flange width B [mm]", GH_ParamAccess.item, 200.0);
            p.AddNumberParameter("H", "H", "Overall height H [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("t_f", "t_f", "Flange thickness t_f [mm]", GH_ParamAccess.item, 8.0);
            p.AddNumberParameter("t_w", "t_w", "Web thickness t_w [mm]", GH_ParamAccess.item, 12.0);
            p.AddNumberParameter("r", "r", "Root radius r [mm]", GH_ParamAccess.item, 6.0);

            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam (CrossSection_H)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int id = 0;
            string name = "H-Section";
            double B = 200.0, H = 100.0, tf = 8.0, tw = 12.0, r = 6.0;

            DA.GetData(0, ref id);
            DA.GetData(1, ref name);
            DA.GetData(2, ref B);
            DA.GetData(3, ref H);
            DA.GetData(4, ref tf);
            DA.GetData(5, ref tw);
            DA.GetData(6, ref r);

            // CrossSection_H は内部で A, Iyy, Izz, J, iy, iz を計算
            CrossSection_Beam xsec = new FEMur.CrossSections.CrossSection_H(id, name ?? "H-Section", B, H, tf, tw, r);

            DA.SetData(0, xsec);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("D7F22023-0A2C-43E6-8579-768307D37A76");
    }
}