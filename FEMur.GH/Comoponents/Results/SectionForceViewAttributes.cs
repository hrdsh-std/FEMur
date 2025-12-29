using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// SectionForceView コンポーネントのカスタムUI属性
    /// 展開可能なタブメニューでチェックボックスとラジオボタンを表示
    /// </summary>
    public class SectionForceViewAttributes : GH_ComponentAttributes
    {
        #region UI Layout Constants (UIレイアウト定数)

        private const float COMPONENT_MARGIN_HORIZONTAL = 2f;
        private const float COMPONENT_MARGIN_VERTICAL = 4f;

        private const float TAB_HEIGHT = 14f;
        private const float TAB_MARGIN_TOP = 4f;

        private const float MENU_LEFT_MARGIN = 5f;
        private const float MENU_TOP_PADDING = 5f;

        private const float CONTROL_SIZE = 10f;
        private const float CONTROL_RIGHT_MARGIN = 25f;
        private const float CONTROL_FILL_MARGIN = 2f;
        private const float RADIO_FILL_MARGIN = 3f;

        private const float LINE_HEIGHT_NORMAL = 14f;
        private const float LINE_HEIGHT_SECTION = 18f;

        private const float MENU_CONTENT_HEIGHT = 118f;

        private static readonly Color TAB_BACKGROUND_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color TAB_TEXT_COLOR = Color.White;

        #endregion

        #region Fields

        // 名前を変えて隠蔽回避（型付きOwner)
        private SectionForceView Cmp => base.Owner as SectionForceView;

        private RectangleF sectionForcesArea;
        private RectangleF filledCheckBox;
        private RectangleF numbersCheckBox;
        private RectangleF fxRadio;
        private RectangleF fyRadio;
        private RectangleF fzRadio;
        private RectangleF mxRadio;
        private RectangleF myRadio;
        private RectangleF mzRadio;

        #endregion

        public SectionForceViewAttributes(SectionForceView owner) : base(owner) { }

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);

            float extraHeight = TAB_HEIGHT + TAB_MARGIN_TOP;
            if (Cmp.IsSectionForcesTabExpanded)
            {
                extraHeight += MENU_CONTENT_HEIGHT;
            }

            bounds.Height += extraHeight;
            Bounds = bounds;

            float tabY = bounds.Bottom - extraHeight + TAB_MARGIN_TOP;
            sectionForcesArea = new RectangleF(
                bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                tabY,
                bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                TAB_HEIGHT
            );

            if (Cmp.IsSectionForcesTabExpanded)
            {
                float currentY = sectionForcesArea.Bottom + MENU_TOP_PADDING;
                float leftMargin = bounds.Left + MENU_LEFT_MARGIN;
                float rightPosition = leftMargin + bounds.Width - CONTROL_RIGHT_MARGIN;

                filledCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                numbersCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_SECTION;

                fxRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                fyRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                fzRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                mxRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                myRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                mzRadio = CreateControlRect(rightPosition, currentY);
            }
        }

        private RectangleF CreateControlRect(float x, float y)
        {
            return new RectangleF(x, y, CONTROL_SIZE, CONTROL_SIZE);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                RenderTab(graphics);

                if (Cmp.IsSectionForcesTabExpanded)
                {
                    RenderMenuContent(graphics);
                }
            }
        }

        private void RenderTab(Graphics graphics)
        {
            GH_Palette palette = GH_Palette.Normal;
            if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error)
                palette = GH_Palette.Error;
            else if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning)
                palette = GH_Palette.Warning;

            GH_Capsule tabCapsule = GH_Capsule.CreateCapsule(sectionForcesArea, palette);
            tabCapsule.Render(graphics, Selected, Cmp.Locked, false);
            tabCapsule.Dispose();

            using (SolidBrush darkBrush = new SolidBrush(TAB_BACKGROUND_COLOR))
            {
                graphics.FillRectangle(darkBrush, sectionForcesArea);
            }

            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using (SolidBrush textBrush = new SolidBrush(TAB_TEXT_COLOR))
            {
                graphics.DrawString("Section Forces", GH_FontServer.Small, textBrush, sectionForcesArea, format);
            }
        }

        private void RenderMenuContent(Graphics graphics)
        {
            var font = GH_FontServer.Small;
            float leftMargin = Bounds.Left + MENU_LEFT_MARGIN;

            using (SolidBrush textBrush = new SolidBrush(TEXT_COLOR))
            {
                graphics.DrawString("Filled", font, textBrush, leftMargin, filledCheckBox.Top);
                DrawCheckBox(graphics, filledCheckBox, Cmp.ShowFilled);

                graphics.DrawString("Numbers", font, textBrush, leftMargin, numbersCheckBox.Top);
                DrawCheckBox(graphics, numbersCheckBox, Cmp.ShowNumbers);

                DrawRadioButtonWithLabel(graphics, font, textBrush, leftMargin, "Fx", fxRadio,
                    Cmp.SelectedForceType == SectionForceView.SectionForceType.Fx);
                DrawRadioButtonWithLabel(graphics, font, textBrush, leftMargin, "Fy", fyRadio,
                    Cmp.SelectedForceType == SectionForceView.SectionForceType.Fy);
                DrawRadioButtonWithLabel(graphics, font, textBrush, leftMargin, "Fz", fzRadio,
                    Cmp.SelectedForceType == SectionForceView.SectionForceType.Fz);
                DrawRadioButtonWithLabel(graphics, font, textBrush, leftMargin, "Mx", mxRadio,
                    Cmp.SelectedForceType == SectionForceView.SectionForceType.Mx);
                DrawRadioButtonWithLabel(graphics, font, textBrush, leftMargin, "My", myRadio,
                    Cmp.SelectedForceType == SectionForceView.SectionForceType.My);
                DrawRadioButtonWithLabel(graphics, font, textBrush, leftMargin, "Mz", mzRadio,
                    Cmp.SelectedForceType == SectionForceView.SectionForceType.Mz);
            }
        }

        private void DrawRadioButtonWithLabel(Graphics graphics, Font font, Brush textBrush,
            float leftMargin, string label, RectangleF rect, bool isSelected)
        {
            graphics.DrawString(label, font, textBrush, leftMargin, rect.Top);
            DrawRadioButton(graphics, rect, isSelected);
        }

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

        private void DrawRadioButton(Graphics graphics, RectangleF rect, bool isSelected)
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.FillEllipse(Brushes.White, rect);
            graphics.DrawEllipse(Pens.Black, rect);

            if (isSelected)
            {
                RectangleF innerRect = new RectangleF(
                    rect.X + RADIO_FILL_MARGIN,
                    rect.Y + RADIO_FILL_MARGIN,
                    rect.Width - (RADIO_FILL_MARGIN * 2),
                    rect.Height - (RADIO_FILL_MARGIN * 2)
                );
                using (SolidBrush fillBrush = new SolidBrush(CONTROL_FILL_COLOR))
                {
                    graphics.FillEllipse(fillBrush, innerRect);
                }
            }

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // タブクリックで展開/折りたたみ
                if (sectionForcesArea.Contains(e.CanvasLocation))
                {
                    Cmp.IsSectionForcesTabExpanded = !Cmp.IsSectionForcesTabExpanded;
                    Cmp.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }

                // 展開時のみチェックボックス/ラジオボタン処理
                if (Cmp.IsSectionForcesTabExpanded)
                {
                    // Filled チェックボックス
                    if (filledCheckBox.Contains(e.CanvasLocation))
                    {
                        Cmp.ShowFilled = !Cmp.ShowFilled;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    // Numbers チェックボックス
                    if (numbersCheckBox.Contains(e.CanvasLocation))
                    {
                        Cmp.ShowNumbers = !Cmp.ShowNumbers;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    // 断面力タイプのラジオボタン
                    if (fxRadio.Contains(e.CanvasLocation))
                    {
                        Cmp.SelectedForceType = SectionForceView.SectionForceType.Fx;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (fyRadio.Contains(e.CanvasLocation))
                    {
                        Cmp.SelectedForceType = SectionForceView.SectionForceType.Fy;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (fzRadio.Contains(e.CanvasLocation))
                    {
                        Cmp.SelectedForceType = SectionForceView.SectionForceType.Fz;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (mxRadio.Contains(e.CanvasLocation))
                    {
                        Cmp.SelectedForceType = SectionForceView.SectionForceType.Mx;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (myRadio.Contains(e.CanvasLocation))
                    {
                        Cmp.SelectedForceType = SectionForceView.SectionForceType.My;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (mzRadio.Contains(e.CanvasLocation))
                    {
                        Cmp.SelectedForceType = SectionForceView.SectionForceType.Mz;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
}