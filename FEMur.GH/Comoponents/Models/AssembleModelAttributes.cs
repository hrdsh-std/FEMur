using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace FEMurGH.Comoponents.Models
{
    /// <summary>
    /// AssembleModel コンポーネントのカスタムUI属性
    /// 折り畳み可能なタブメニューでチェックボックスを表示
    /// </summary>
    public class AssembleModelAttributes : GH_ComponentAttributes
    {
        #region UI Layout Constants

        private const float COMPONENT_MARGIN_HORIZONTAL = 2f;
        private const float COMPONENT_MARGIN_VERTICAL = 4f;

        private const float TAB_HEIGHT = 14f;
        private const float TAB_MARGIN_TOP = 4f;

        private const float MENU_LEFT_MARGIN = 8f;
        private const float MENU_TOP_PADDING = 5f;
        private const float MENU_BOTTOM_PADDING = 3f;

        private const float CONTROL_SIZE = 10f;
        private const float CONTROL_RIGHT_MARGIN = 15f;
        private const float CONTROL_FILL_MARGIN = 2f;

        private const float LINE_HEIGHT_NORMAL = 14f;

        private const float MENU_CONTENT_HEIGHT = 90f; // 6 checkboxes with padding

        private static readonly Color TAB_BACKGROUND_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color TAB_TEXT_COLOR = Color.White;

        #endregion

        #region Fields

        private AssembleModel Cmp => base.Owner as AssembleModel;

        private RectangleF displayArea;
        private RectangleF nodeIdCheckBox;
        private RectangleF elementIdCheckBox;
        private RectangleF loadCheckBox;
        private RectangleF supportCheckBox;
        private RectangleF localAxisCheckBox;
        private RectangleF crossSectionCheckBox;

        #endregion

        public AssembleModelAttributes(AssembleModel owner) : base(owner) { }

        #region Layout Methods

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);

            float extraHeight = TAB_HEIGHT + TAB_MARGIN_TOP;
            if (Cmp.IsDisplayTabExpanded)
            {
                extraHeight += MENU_CONTENT_HEIGHT;
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
                }
            }
        }

        /// <summary>
        /// Displayタブを描画
        /// </summary>
        private void RenderTab(Graphics graphics)
        {
            GH_Palette palette = GH_Palette.Normal;
            if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error)
                palette = GH_Palette.Error;
            else if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning)
                palette = GH_Palette.Warning;

            GH_Capsule tabCapsule = GH_Capsule.CreateCapsule(displayArea, palette);
            tabCapsule.Render(graphics, Selected, Cmp.Locked, false);
            tabCapsule.Dispose();

            using (SolidBrush darkBrush = new SolidBrush(TAB_BACKGROUND_COLOR))
            {
                graphics.FillRectangle(darkBrush, displayArea);
            }

            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using (SolidBrush textBrush = new SolidBrush(TAB_TEXT_COLOR))
            {
                graphics.DrawString("Display", GH_FontServer.Small, textBrush, displayArea, format);
            }
        }

        /// <summary>
        /// メニューコンテンツを描画
        /// </summary>
        private void RenderMenuContent(Graphics graphics)
        {
            var font = GH_FontServer.Small;
            float leftMargin = Bounds.Left + MENU_LEFT_MARGIN;

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
            }
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
                // タブクリックで展開/折りたたみ
                if (displayArea.Contains(e.CanvasLocation))
                {
                    Cmp.IsDisplayTabExpanded = !Cmp.IsDisplayTabExpanded;
                    Cmp.ExpireSolution(true);
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
                }
            }

            return base.RespondToMouseDown(sender, e);
        }

        /// <summary>
        /// チェックボックスのクリック処理
        /// </summary>
        private bool HandleCheckBoxClick(RectangleF rect, PointF location, Func<bool> getter, Action<bool> setter)
        {
            if (rect.Contains(location))
            {
                var current = getter();
                setter(!current);
                Cmp.ExpireSolution(true);
                return true;
            }
            return false;
        }

        #endregion
    }
}