using System;
using Grasshopper.Kernel;
using FEMur.CrossSections;

namespace FEMurGH.CrossSections
{
    /// <summary>
    /// 円形断面（中空）の CrossSection を生成するコンポーネント。
    /// 入力: Id, Name, D, t
    /// 出力: CrossSection_Beam（実体は CrossSection_Circle）
    /// </summary>
    public class CrossSection_Circle : GH_Component
    {
        public CrossSection_Circle()
          : base("CrossSection Circle (FEMur)", "XSec-Circle",
              "Create circular pipe section (CrossSection_Circle) for beam elements.",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddIntegerParameter("Id", "Id", "CrossSection Id", GH_ParamAccess.item, 0);
            p.AddTextParameter("Name", "Name", "CrossSection name", GH_ParamAccess.item, "Circle-Section");

            p.AddNumberParameter("D", "D", "Outer diameter D [mm]", GH_ParamAccess.item, 100.0);
            p.AddNumberParameter("t", "t", "Wall thickness t [mm]", GH_ParamAccess.item, 5.0);

            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam (CrossSection_Circle)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int id = 0;
            string name = "Circle-Section";
            double D = 100.0, t = 5.0;

            DA.GetData(0, ref id);
            DA.GetData(1, ref name);
            DA.GetData(2, ref D);
            DA.GetData(3, ref t);

            // CrossSection_Circle は内部で A, Iyy, Izz, J, iy, iz を計算
            CrossSection_Beam xsec = new FEMur.CrossSections.CrossSection_Circle(id, name ?? "Circle-Section", D, t);

            DA.SetData(0, xsec);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("AB46861C-6691-4419-BCC4-64B1A3CDC66E");
    }
}