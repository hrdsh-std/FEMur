using FEMur.Common.Units;
using FEMurGH.Comoponents.Results;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FEMurGH.Comoponents.Models
{
    /// <summary>
    /// AssembleModel コンポーネントのカスタムUI属性
    /// 展開可能なタブメニューでチェックボックスを表示
    /// </summary>
    public class AssembleModelAttributes : GH_ComponentAttributes
    {
        #region UI Layout Constants

        private const float COMPONENT_MARGIN_HORIZONTAL = 2f;
        private const float COMPONENT_MARGIN_VERTICAL = 2f;

        private const float TAB_HEIGHT = 14f;
        private const float TAB_MARGIN_TOP = 4f;
        private const float TAB_MARGIN_BOTTOM = 2f;

        private const float MENU_LEFT_MARGIN = 8f;
        private const float MENU_TOP_PADDING = 4f;
        private const float MENU_BOTTOM_PADDING = 3f;

        private const float CONTROL_SIZE = 10f;
        private const float CONTROL_RIGHT_MARGIN = 15f;
        private const float CONTROL_FILL_MARGIN = 2f;

        private const float LINE_HEIGHT_NORMAL = 15f;
        private const float SECTION_SPACING = 8f;

        // チェックボックスの数と各行の高さから動的に計算
        private const int CHECKBOX_COUNT = 7; // NodeID, ElementID, Load, Support, LocalAxis, CrossSection, Joint
        private float MENU_CONTENT_HEIGHT => (CHECKBOX_COUNT * LINE_HEIGHT_NORMAL) + MENU_TOP_PADDING ;

        // 単位選択エリアの定数
        private const float UNIT_DROPDOWN_HEIGHT = 16f;
        private const float UNIT_SPACING = 2f;
        private const float UNIT_AREA_HEIGHT = UNIT_DROPDOWN_HEIGHT + UNIT_SPACING;

        private static readonly Color TAB_BACKGROUND_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color TAB_TEXT_COLOR = Color.White;
        private static readonly Color GROUP_BACKGROUND_COLOR = Color.FromArgb(240, 240, 240);

        #endregion

        #region Fields

        private AssembleModel Cmp => base.Owner as AssembleModel;

        private RectangleF displayArea;
        
        // チェックボックスグループ
        private RectangleF checkboxGroupArea;
        private RectangleF nodeIdCheckBox;
        private RectangleF elementIdCheckBox;
        private RectangleF loadCheckBox;
        private RectangleF supportCheckBox;
        private RectangleF localAxisCheckBox;
        private RectangleF crossSectionCheckBox;
        private RectangleF jointCheckBox;

        // 単位選択用のフィールド
        private RectangleF _forceUnitDropdownBounds;
        private RectangleF _lengthUnitDropdownBounds;

        #endregion

        public AssembleModelAttributes(AssembleModel owner) : base(owner) { }

        #region Layout Methods

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);

            float extraHeight = TAB_HEIGHT + TAB_MARGIN_TOP + TAB_MARGIN_BOTTOM;
            if (Cmp.IsDisplayTabExpanded)
            {
                extraHeight += MENU_CONTENT_HEIGHT;
                extraHeight += UNIT_AREA_HEIGHT;
            }

            bounds.Height += extraHeight;
            Bounds = bounds;

            float tabY = bounds.Bottom - extraHeight + TAB_MARGIN_TOP;
            displayArea = new RectangleF(
                bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                tabY,
                bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                TAB_HEIGHT
            );

            if (Cmp.IsDisplayTabExpanded)
            {
                float currentY = displayArea.Bottom + MENU_TOP_PADDING;
                float leftMargin = bounds.Left + MENU_LEFT_MARGIN;
                float rightPosition = bounds.Right - CONTROL_RIGHT_MARGIN;

                // チェックボックスグループ
                float checkboxGroupTop = currentY;

                nodeIdCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                elementIdCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                loadCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                supportCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                localAxisCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                crossSectionCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                jointCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                checkboxGroupArea = new RectangleF(
                    bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                    checkboxGroupTop - 2,
                    bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                    (CHECKBOX_COUNT * LINE_HEIGHT_NORMAL) + 2
                );

                // 単位選択エリアのレイアウト
                float unitStartY = checkboxGroupArea.Bottom + UNIT_SPACING;
                float centerX = bounds.Left + bounds.Width / 2;

                float dropdownWidth = (bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2)) / 2 - UNIT_SPACING / 2;
                float totalWidth = dropdownWidth * 2 + UNIT_SPACING;
                float leftX = bounds.Left + COMPONENT_MARGIN_HORIZONTAL;
                
                _forceUnitDropdownBounds = new RectangleF(
                    leftX,
                    unitStartY,
                    dropdownWidth,
                    UNIT_DROPDOWN_HEIGHT
                );
                
                _lengthUnitDropdownBounds = new RectangleF(
                    leftX + dropdownWidth + UNIT_SPACING,
                    unitStartY,
                    dropdownWidth,
                    UNIT_DROPDOWN_HEIGHT
                );
            }
            else
            {
                _forceUnitDropdownBounds = RectangleF.Empty;
                _lengthUnitDropdownBounds = RectangleF.Empty;
            }
        }

        private RectangleF CreateControlRect(float x, float y)
        {
            return new RectangleF(x, y, CONTROL_SIZE, CONTROL_SIZE);
        }

        #endregion

        #region Rendering Methods

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                RenderTab(graphics);

                if (Cmp.IsDisplayTabExpanded)
                {
                    RenderMenuContent(graphics);
                    RenderUnitSelection(graphics);
                }
            }
        }

        /// <summary>
        /// Displayタブを描画
        /// </summary>
        private void RenderTab(Graphics graphics)
        {
            GH_Palette palette = GH_Palette.Black;

            // タブ全体のカプセルを描画
            GH_Capsule tabCapsule = GH_Capsule.CreateCapsule(displayArea, palette, 2, 4);
            tabCapsule.Render(graphics, Selected, Cmp.Locked, false);
            tabCapsule.Dispose();

            // タブテキストの描画
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            
            using (SolidBrush textBrush = new SolidBrush(TAB_TEXT_COLOR))
            {
                graphics.DrawString("Display", GH_FontServer.StandardBold, textBrush, displayArea, format);
            }
        }

        /// <summary>
        /// メニューコンテンツを描画
        /// </summary>
        private void RenderMenuContent(Graphics graphics)
        {
            var font = GH_FontServer.Small;
            float leftMargin = Bounds.Left + MENU_LEFT_MARGIN;

            // チェックボックスグループのカプセル

            GH_Palette palette = GH_CapsuleRenderEngine.GetImpliedPalette(Cmp);

            GH_Capsule checkboxCapsule = GH_Capsule.CreateCapsule(checkboxGroupArea, palette, 2, 0);
            
            bool isHidden = false;
            if (Cmp is IGH_Component ghComponent)
            {
                isHidden = ghComponent.Hidden;
            }
            
            checkboxCapsule.Render(graphics, Selected, Cmp.Locked, isHidden);
            checkboxCapsule.Dispose();

            using (SolidBrush textBrush = new SolidBrush(TEXT_COLOR))
            {
                graphics.DrawString("NodeID", font, textBrush, leftMargin, nodeIdCheckBox.Top);
                DrawCheckBox(graphics, nodeIdCheckBox, Cmp.ShowNodeId);

                graphics.DrawString("ElementID", font, textBrush, leftMargin, elementIdCheckBox.Top);
                DrawCheckBox(graphics, elementIdCheckBox, Cmp.ShowElementId);

                graphics.DrawString("Load", font, textBrush, leftMargin, loadCheckBox.Top);
                DrawCheckBox(graphics, loadCheckBox, Cmp.ShowLoad);

                graphics.DrawString("Support", font, textBrush, leftMargin, supportCheckBox.Top);
                DrawCheckBox(graphics, supportCheckBox, Cmp.ShowSupport);

                graphics.DrawString("LocalAxis", font, textBrush, leftMargin, localAxisCheckBox.Top);
                DrawCheckBox(graphics, localAxisCheckBox, Cmp.ShowLocalAxis);

                graphics.DrawString("CrossSection", font, textBrush, leftMargin, crossSectionCheckBox.Top);
                DrawCheckBox(graphics, crossSectionCheckBox, Cmp.ShowCrossSection);

                graphics.DrawString("Joint", font, textBrush, leftMargin, jointCheckBox.Top);
                DrawCheckBox(graphics, jointCheckBox, Cmp.ShowJoint);
            }
        }

        private void RenderUnitSelection(Graphics graphics)
        {
            // DropdownRendererヘルパークラスを使用
            DropdownRenderer.DrawDropdown(graphics, _forceUnitDropdownBounds, 
                Cmp.SelectedForceUnit.ToString(), Cmp, Selected);
            DropdownRenderer.DrawDropdown(graphics, _lengthUnitDropdownBounds, 
                Cmp.SelectedLengthUnit.ToString(), Cmp, Selected);
        }

        /// <summary>
        /// チェックボックスを描画
        /// </summary>
        private void DrawCheckBox(Graphics graphics, RectangleF rect, bool isChecked)
        {
            graphics.FillRectangle(Brushes.White, rect);
            graphics.DrawRectangle(Pens.Black, Rectangle.Round(rect));

            if (isChecked)
            {
                RectangleF innerRect = new RectangleF(
                    rect.X + CONTROL_FILL_MARGIN,
                    rect.Y + CONTROL_FILL_MARGIN,
                    rect.Width - (CONTROL_FILL_MARGIN * 2),
                    rect.Height - (CONTROL_FILL_MARGIN * 2)
                );
                using (SolidBrush fillBrush = new SolidBrush(CONTROL_FILL_COLOR))
                {
                    graphics.FillRectangle(fillBrush, innerRect);
                }
            }
        }

        #endregion

        #region Event Handlers

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (_forceUnitDropdownBounds.Contains(e.CanvasLocation))
                {
                    ShowForceUnitMenu(e.CanvasLocation);
                    return GH_ObjectResponse.Handled;
                }
                
                if (_lengthUnitDropdownBounds.Contains(e.CanvasLocation))
                {
                    ShowLengthUnitMenu(e.CanvasLocation);
                    return GH_ObjectResponse.Handled;
                }

                // タブクリックで展開/折りたたみ
                if (displayArea.Contains(e.CanvasLocation))
                {
                    Cmp.IsDisplayTabExpanded = !Cmp.IsDisplayTabExpanded;
                    
                    // レイアウトを再計算
                    Layout();
                    
                    // キャンバスを再描画
                    sender.Refresh();
                    
                    return GH_ObjectResponse.Handled;
                }

                // 展開時のみチェックボックス処理
                if (Cmp.IsDisplayTabExpanded)
                {
                    if (HandleCheckBoxClick(nodeIdCheckBox, e.CanvasLocation, () => Cmp.ShowNodeId, v => Cmp.ShowNodeId = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleCheckBoxClick(elementIdCheckBox, e.CanvasLocation, () => Cmp.ShowElementId, v => Cmp.ShowElementId = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleCheckBoxClick(loadCheckBox, e.CanvasLocation, () => Cmp.ShowLoad, v => Cmp.ShowLoad = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleCheckBoxClick(supportCheckBox, e.CanvasLocation, () => Cmp.ShowSupport, v => Cmp.ShowSupport = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleCheckBoxClick(localAxisCheckBox, e.CanvasLocation, () => Cmp.ShowLocalAxis, v => Cmp.ShowLocalAxis = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleCheckBoxClick(crossSectionCheckBox, e.CanvasLocation, () => Cmp.ShowCrossSection, v => Cmp.ShowCrossSection = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleCheckBoxClick(jointCheckBox, e.CanvasLocation, () => Cmp.ShowJoint, v => Cmp.ShowJoint = v))
                        return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }

        /// <summary>
        /// チェックボックスのクリック処理（キャッシュ更新とビューポート再描画）
        /// </summary>
        private bool HandleCheckBoxClick(RectangleF rect, PointF location, Func<bool> getter, Action<bool> setter)
        {
            if (rect.Contains(location))
            {
                setter(!getter());
                
                // モデル再構築ではなく、キャッシュ更新のみ
                if (Cmp._cachedModel != null)
                {
                    Cmp.UpdateCaches(Cmp._cachedModel);
                }
                
                // Rhinoビューポートのプレビューを一旦切りにして再描画する
                Cmp.ExpirePreview(true);
                
                // Rhinoのすべてのビューポートを即座に再描画
                Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
                
                // Grasshopperキャンバスを再描画
                Grasshopper.Instances.ActiveCanvas?.Refresh();
                
                return true;
            }
            return false;
        }

        private void ShowForceUnitMenu(PointF location)
        {
            var menu = new ToolStripDropDown();
            
            foreach (ForceUnit unit in Enum.GetValues(typeof(ForceUnit)))
            {
                var item = new ToolStripMenuItem(unit.ToString());
                item.Checked = (Cmp.SelectedForceUnit == unit);
                item.Click += (s, e) =>
                {
                    Cmp.SelectedForceUnit = unit;
                    
                    // Rhinoビューポートを再描画（単位表示が変わるため）
                    Cmp.ExpirePreview(true);
                    Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
                    
                    // Grasshopperキャンバスを再描画
                    Grasshopper.Instances.ActiveCanvas?.Refresh();
                };
                menu.Items.Add(item);
            }
            
            GH_Canvas canvas = Grasshopper.Instances.ActiveCanvas;
            
            PointF canvasPoint = new PointF(
                _forceUnitDropdownBounds.Left,
                _forceUnitDropdownBounds.Bottom
            );
            
            PointF controlPoint = canvas.Viewport.ProjectPoint(canvasPoint);
            Point screenLocation = canvas.PointToScreen(Point.Round(controlPoint));
            
            menu.Show(screenLocation);
        }

        private void ShowLengthUnitMenu(PointF location)
        {
            var menu = new ToolStripDropDown();
            
            foreach (LengthUnit unit in Enum.GetValues(typeof(LengthUnit)))
            {
                var item = new ToolStripMenuItem(unit.ToString());
                item.Checked = (Cmp.SelectedLengthUnit == unit);
                item.Click += (s, e) =>
                {
                    Cmp.SelectedLengthUnit = unit;
                    
                    // Rhinoビューポートを再描画
                    Cmp.ExpirePreview(true);
                    Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
                    
                    // Grasshopperキャンバスを再描画
                    Grasshopper.Instances.ActiveCanvas?.Refresh();
                };
                menu.Items.Add(item);
            }
            
            GH_Canvas canvas = Grasshopper.Instances.ActiveCanvas;
            
            PointF canvasPoint = new PointF(
                _lengthUnitDropdownBounds.Left,
                _lengthUnitDropdownBounds.Bottom
            );
            
            PointF controlPoint = canvas.Viewport.ProjectPoint(canvasPoint);
            Point screenLocation = canvas.PointToScreen(Point.Round(controlPoint));
            
            menu.Show(screenLocation);
        }

        #endregion
    }
}