using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using FEMur.Materials;

namespace FEMurGH.Materials
{
    /// <summary>
    /// Material コンポーネント
    /// - ドロップダウンで材質（等方/直交異方）とプリセット選択
    /// - 入力ピンでユーザー定義（未接続はプリセット値を使用）
    /// - 出力は FEMur.Materials.Material（実体は Material_Isotropic または Material_Orthotropic）
    /// </summary>
    public class Material : GH_Component
    {
        private enum MaterialModel { Isotropic, Orthotropic }
        private enum IsoPreset { Custom, Steel, Aluminum, Concrete }

        // メニュー状態（永続化）
        private MaterialModel _model = MaterialModel.Isotropic;
        private IsoPreset _isoPreset = IsoPreset.Steel;

        public Material()
          : base(
              "Material(FEMur)",
              "Material",
              "Create FEMur Material. Choose preset from dropdown, optionally override by inputs.",
              "FEMur",
              "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            // 共通
            p.AddIntegerParameter("Id", "Id", "Material Id", GH_ParamAccess.item, 0);
            p.AddTextParameter("Family", "Fam", "Material family (e.g., Isotropic, Orthotropic)", GH_ParamAccess.item, string.Empty);
            p.AddTextParameter("Name", "Name", "Material name", GH_ParamAccess.item, string.Empty);
            p.AddNumberParameter("Density", "Rho", "Density [t/mm^3] (optional)", GH_ParamAccess.item);

            // 等方弾性（未接続時はプリセット）
            p.AddNumberParameter("E", "E", "Young's modulus [N/mm^2] (isotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("nu", "nu", "Poisson's ratio (isotropic)", GH_ParamAccess.item);

            // 直交異方（未接続時は Custom デフォルト）
            p.AddNumberParameter("Ex", "Ex", "Ex [N/mm^2] (orthotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("Ey", "Ey", "Ey [N/mm^2] (orthotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("Ez", "Ez", "Ez [N/mm^2] (orthotropic)", GH_ParamAccess.item);

            p.AddNumberParameter("Gxy", "Gxy", "Gxy [N/mm^2] (orthotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("Gyz", "Gyz", "Gyz [N/mm^2] (orthotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("Gzx", "Gzx", "Gzx [N/mm^2] (orthotropic)", GH_ParamAccess.item);

            p.AddNumberParameter("nu_xy", "nu_xy", "nu_xy (orthotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("nu_yz", "nu_yz", "nu_yz (orthotropic)", GH_ParamAccess.item);
            p.AddNumberParameter("nu_zx", "nu_zx", "nu_zx (orthotropic)", GH_ParamAccess.item);

            // すべて任意入力（未接続はプリセット使用）
            for (int i = 0; i < p.ParamCount; i++)
                p[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("Material", "Mat", "FEMur Material (isotropic/orthotropic)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 共通入力（Optional）
            int id = 0;
            string family = null;
            string name = null;

            double density = 0.0;
            bool hasRho = DA.GetData(3, ref density);

            DA.GetData(0, ref id);
            DA.GetData(1, ref family);
            DA.GetData(2, ref name);

            if (_model == MaterialModel.Isotropic)
            {
                // プリセット初期値
                double defE, defNu, defRho;
                string defName;
                GetIsotropicPreset(_isoPreset, out defE, out defNu, out defRho, out defName);

                double E = defE, nu = defNu;
                bool hasE = DA.GetData(4, ref E);
                bool hasNu = DA.GetData(5, ref nu);

                // Family/Name 未指定ならプリセット適用
                if (string.IsNullOrWhiteSpace(family)) family = "Isotropic";
                if (string.IsNullOrWhiteSpace(name)) name = defName;

                // 密度未指定ならプリセット密度
                double rho = hasRho ? density : defRho;

                // 等方材を生成
                var mat = new Material_Isotropic(
                    id: id,
                    family: family,
                    name: name,
                    E: E,
                    nu: nu,
                    density: rho
                );

                DA.SetData(0, mat);
            }
            else
            {
                // 直交異方：未接続時のデフォルト（Custom）
                double Ex = 10000.0, Ey = 1000.0, Ez = 1000.0;
                double Gxy = 500.0, Gyz = 300.0, Gzx = 300.0;
                double nuxy = 0.3, nuyz = 0.3, nuzx = 0.3;
                double rho = hasRho ? density : 6.0e-10;

                DA.GetData(6, ref Ex);
                DA.GetData(7, ref Ey);
                DA.GetData(8, ref Ez);

                DA.GetData(9, ref Gxy);
                DA.GetData(10, ref Gyz);
                DA.GetData(11, ref Gzx);

                DA.GetData(12, ref nuxy);
                DA.GetData(13, ref nuyz);
                DA.GetData(14, ref nuzx);

                if (string.IsNullOrWhiteSpace(family)) family = "Orthotropic";
                if (string.IsNullOrWhiteSpace(name)) name = "Custom-Orthotropic";

                var mat = new Material_Orthotropic(
                    id: id,
                    family: family,
                    name: name,
                    density: rho,
                    ex: Ex, ey: Ey, ez: Ez,
                    gxy: Gxy, gyz: Gyz, gzx: Gzx,
                    nuxy: nuxy, nuyz: nuyz, nuzx: nuzx
                );

                DA.SetData(0, mat);
            }
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
                    name = "Custom-Isotropic";
                    E = 210000.0; nu = 0.30; rho = 7.85e-9;
                    break;
            }
        }

        // 右クリックメニューにドロップダウンを追加
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            // モデル選択
            var modelMenu = new ToolStripMenuItem("Material Model");
            foreach (MaterialModel m in Enum.GetValues(typeof(MaterialModel)))
            {
                var item = new ToolStripMenuItem(m.ToString())
                {
                    Checked = (_model == m),
                    Tag = m
                };
                item.Click += (s, e) =>
                {
                    _model = (MaterialModel)((ToolStripMenuItem)s).Tag;
                    ExpireSolution(true);
                };
                modelMenu.DropDownItems.Add(item);
            }
            menu.Items.Add(modelMenu);

            // 等方プリセット（等方選択時のみ活性）
            var presetMenu = new ToolStripMenuItem("Isotropic Preset") { Enabled = (_model == MaterialModel.Isotropic) };
            foreach (IsoPreset p in Enum.GetValues(typeof(IsoPreset)))
            {
                var item = new ToolStripMenuItem(p.ToString())
                {
                    Checked = (_isoPreset == p),
                    Tag = p,
                    Enabled = (_model == MaterialModel.Isotropic)
                };
                item.Click += (s, e) =>
                {
                    _isoPreset = (IsoPreset)((ToolStripMenuItem)s).Tag;
                    ExpireSolution(true);
                };
                presetMenu.DropDownItems.Add(item);
            }
            menu.Items.Add(presetMenu);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("FB8FBEB8-0F0D-4290-A299-2F28DDF88A27");
    }
}