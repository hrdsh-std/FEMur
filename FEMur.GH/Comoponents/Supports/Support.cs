using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Supports;
using FEMur.Geometry;

namespace FEMurGH.Comoponents.Supports
{
    public class Support : GH_Component
    {
        // 拘束条件の状態を保持
        private bool _ux = false;
        private bool _uy = false;
        private bool _uz = false;
        private bool _rx = false;
        private bool _ry = false;
        private bool _rz = false;

        // 公開プロパティ（Attributes からアクセス可能）
        public bool UX => _ux;
        public bool UY => _uy;
        public bool UZ => _uz;
        public bool RX => _rx;
        public bool RY => _ry;
        public bool RZ => _rz;

        /// <summary>
        /// Initializes a new instance of the Support class.
        /// </summary>
        public Support()
          : base("Support(FEMur)", "Support",
              "Create FEMur Supports from Points with constraint conditions",
              "FEMur", "6.Support")
        {
        }

        /// <summary>
        /// カスタム UI Attributes を作成
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new SupportAttributes(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points where supports are applied (will match with existing nodes)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Supports", "S", "FEMur Supports", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var points = new List<Point3d>();
            if (!DA.GetDataList(0, points) || points.Count == 0)
                return;

            var supports = new List<FEMur.Supports.Support>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                
                // Rhino Point3d を FEMur Point3 に変換
                var femurPoint = new Point3(pt.X, pt.Y, pt.Z);
                
                // Support(Point3 position, bool fixDX, bool fixDY, bool fixDZ, bool fixRX, bool fixRY, bool fixRZ)
                supports.Add(new FEMur.Supports.Support(femurPoint, _ux, _uy, _uz, _rx, _ry, _rz));
            }

            DA.SetDataList(0, supports);
        }

        // トグルメソッド（Attributes から呼び出される）
        public void ToggleUX() { _ux = !_ux; ExpireSolution(true); }
        public void ToggleUY() { _uy = !_uy; ExpireSolution(true); }
        public void ToggleUZ() { _uz = !_uz; ExpireSolution(true); }
        public void ToggleRX() { _rx = !_rx; ExpireSolution(true); }
        public void ToggleRY() { _ry = !_ry; ExpireSolution(true); }
        public void ToggleRZ() { _rz = !_rz; ExpireSolution(true); }

        /// <summary>
        /// 右クリックメニューにプリセットを追加
        /// </summary>
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            // プリセット
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Preset: Pin (UX, UY, UZ)", Menu_PinClicked, true, false);
            Menu_AppendItem(menu, "Preset: Fixed (All)", Menu_FixedClicked, true, false);
            Menu_AppendItem(menu, "Preset: Roller X (UY, UZ)", Menu_RollerXClicked, true, false);
            Menu_AppendItem(menu, "Preset: Roller Y (UX, UZ)", Menu_RollerYClicked, true, false);
            Menu_AppendItem(menu, "Preset: Roller Z (UX, UY)", Menu_RollerZClicked, true, false);
        }

        // プリセットのクリックイベント
        private void Menu_PinClicked(object sender, EventArgs e)
        {
            _ux = _uy = _uz = true;
            _rx = _ry = _rz = false;
            ExpireSolution(true);
        }

        private void Menu_FixedClicked(object sender, EventArgs e)
        {
            _ux = _uy = _uz = true;
            _rx = _ry = _rz = true;
            ExpireSolution(true);
        }

        private void Menu_RollerXClicked(object sender, EventArgs e)
        {
            _ux = false;
            _uy = _uz = true;
            _rx = _ry = _rz = false;
            ExpireSolution(true);
        }

        private void Menu_RollerYClicked(object sender, EventArgs e)
        {
            _uy = false;
            _ux = _uz = true;
            _rx = _ry = _rz = false;
            ExpireSolution(true);
        }

        private void Menu_RollerZClicked(object sender, EventArgs e)
        {
            _uz = false;
            _ux = _uy = true;
            _rx = _ry = _rz = false;
            ExpireSolution(true);
        }

        /// <summary>
        /// データの書き込み（保存時）
        /// </summary>
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("UX", _ux);
            writer.SetBoolean("UY", _uy);
            writer.SetBoolean("UZ", _uz);
            writer.SetBoolean("RX", _rx);
            writer.SetBoolean("RY", _ry);
            writer.SetBoolean("RZ", _rz);
            return base.Write(writer);
        }

        /// <summary>
        /// データの読み込み（読込時）
        /// </summary>
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("UX"))
                _ux = reader.GetBoolean("UX");
            if (reader.ItemExists("UY"))
                _uy = reader.GetBoolean("UY");
            if (reader.ItemExists("UZ"))
                _uz = reader.GetBoolean("UZ");
            if (reader.ItemExists("RX"))
                _rx = reader.GetBoolean("RX");
            if (reader.ItemExists("RY"))
                _ry = reader.GetBoolean("RY");
            if (reader.ItemExists("RZ"))
                _rz = reader.GetBoolean("RZ");
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Support;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2A5B0B0D-58CD-4274-BBE0-F0466120B90D");
    }
}