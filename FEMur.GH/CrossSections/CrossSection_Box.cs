using System;
using Grasshopper.Kernel;
using FEMur.CrossSections;

namespace FEMurGH.CrossSections
{
    /// <summary>
    /// 角形鋼管断面の CrossSection を生成するコンポーネント。
    /// 入力: Id, Name, B, H, t, r
    /// 出力: CrossSection_Beam（実体は CrossSection_Box）
    /// </summary>
    public class CrossSection_Box : GH_Component
    {
        public CrossSection_Box()
          : base("CrossSection Box (FEMur)", "XSec-Box",
              "Create Box section (CrossSection_Box) for beam elements.",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddIntegerParameter("Id", "Id", "CrossSection Id", GH_ParamAccess.item, 0);
            p.AddTextParameter("Name", "Name", "CrossSection name", GH_ParamAccess.item, "Box-Section");

            p.AddNumberParameter("B", "B", "Width B [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("H", "H", "Height H [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("t", "t", "Thickness t [mm]", GH_ParamAccess.item, 6.0);
            p.AddNumberParameter("r", "r", "Corner radius r [mm] (optional, default=1.5*t)", GH_ParamAccess.item, -1.0);

            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam (CrossSection_Box)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int id = 0;
            string name = "Box-Section";
            double B = 100.0, H = 100.0, t = 6.0, r = -1.0;

            DA.GetData(0, ref id);
            DA.GetData(1, ref name);
            DA.GetData(2, ref B);
            DA.GetData(3, ref H);
            DA.GetData(4, ref t);
            DA.GetData(5, ref r);

            // CrossSection_Box は内部で A, Iyy, Izz, J, iy, iz を計算
            CrossSection_Beam xsec = new FEMur.CrossSections.CrossSection_Box(id, name ?? "Box-Section", B, H, t, r);

            DA.SetData(0, xsec);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("8B3A4F2E-1C5D-4E9A-B7F3-2D6E8A9C1F4B");
    }
}