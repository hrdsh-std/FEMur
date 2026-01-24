using System;
using System.Windows.Forms;
using Grasshopper.Kernel;
using FEMur.Materials;

namespace FEMurGH.Comoponents.Materials
{
    /// <summary>
    /// Material_Isotropic コンポーネント
    /// - 等方性材料を作成
    /// - プリセット（Steel, Aluminum, Concrete, Custom）から選択可能
    /// </summary>
    public class Material_Isotropic : GH_Component
    {
        private enum IsoPreset { Custom, Steel, Aluminum, Concrete }

        // プリセット状態（永続化）
        private IsoPreset _preset = IsoPreset.Steel;

        public Material_Isotropic()
          : base(
              "Material Isotropic (FEMur)",
              "Mat-Iso",
              "Create isotropic material. Choose preset from right-click menu.",
              "FEMur",
              "3.Material")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Name", "Name", "Material name (optional, uses preset name if empty)", GH_ParamAccess.item, string.Empty);
            p.AddNumberParameter("Density", "Rho", "Density [t/mm^3] (optional, uses preset if not connected)", GH_ParamAccess.item);
            p.AddNumberParameter("E", "E", "Young's modulus [N/mm^2] (optional, uses preset if not connected)", GH_ParamAccess.item);
            p.AddNumberParameter("nu", "nu", "Poisson's ratio (optional, uses preset if not connected)", GH_ParamAccess.item);

            // すべて任意入力（未接続はプリセット使用）
            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("Material", "Mat", "FEMur Material (isotropic)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // プリセット初期値を取得
            double defE, defNu, defRho;
            string defName;
            GetIsotropicPreset(_preset, out defE, out defNu, out defRho, out defName);

            // 入力を取得（未接続の場合はプリセット値を使用）
            string name = null;
            double density = defRho;
            double E = defE;
            double nu = defNu;

            DA.GetData(0, ref name);
            DA.GetData(1, ref density);
            DA.GetData(2, ref E);
            DA.GetData(3, ref nu);

            // Name未指定ならプリセット名を使用
            if (string.IsNullOrWhiteSpace(name))
            {
                name = defName;
            }

            // Material_Isotropic を作成（Idなし）
            var mat = new FEMur.Materials.Material_Isotropic("Isotropic", name, density, E, nu);

            DA.SetData(0, mat);
        }

        private static void GetIsotropicPreset(IsoPreset preset, out double E, out double nu, out double rho, out string name)
        {
            switch (preset)
            {
                default:
                case IsoPreset.Steel:
                    name = "Steel";
                    E = 210000.0; nu = 0.30; rho = 7.85e-9; // N-mm 系
                    break;
                case IsoPreset.Aluminum:
                    name = "Aluminum";
                    E = 70000.0; nu = 0.33; rho = 2.70e-9;
                    break;
                case IsoPreset.Concrete:
                    name = "Concrete";
                    E = 30000.0; nu = 0.20; rho = 2.40e-9;
                    break;
                case IsoPreset.Custom:
                    name = "Custom";
                    E = 210000.0; nu = 0.30; rho = 7.85e-9;
                    break;
            }
        }

        // 右クリックメニューにプリセットを追加
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendSeparator(menu);
            
            foreach (IsoPreset p in Enum.GetValues(typeof(IsoPreset)))
            {
                Menu_AppendItem(menu, $"Preset: {p}", (s, e) =>
                {
                    _preset = p;
                    ExpireSolution(true);
                }, true, _preset == p);
            }
        }

        /// <summary>
        /// データの書き込み（保存時）
        /// </summary>
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("Preset", (int)_preset);
            return base.Write(writer);
        }

        /// <summary>
        /// データの読み込み（読込時）
        /// </summary>
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("Preset"))
                _preset = (IsoPreset)reader.GetInt32("Preset");
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("FB8FBEB8-0F0D-4290-A299-2F28DDF88A27");
    }
}