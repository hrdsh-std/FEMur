using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using FEMur.Common.Units;

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
        private const float COMPONENT_MARGIN_VERTICAL = 2f;

        private const float TAB_HEIGHT = 14f;
        private const float TAB_MARGIN_TOP = 4f;
        private const float TAB_MARGIN_BOTTOM = 2f;

        private const float MENU_LEFT_MARGIN = 8f;
        private const float MENU_TOP_PADDING = 4f;
        private const float MENU_BOTTOM_PADDING = 3f;
        private const float GROUP_SPACING = 4f;

        private const float CONTROL_SIZE = 10f;
        private const float CONTROL_RIGHT_MARGIN = 15f;
        private const float CONTROL_FILL_MARGIN = 2f;
        private const float RADIO_FILL_MARGIN = 3f;

        private const float LINE_HEIGHT_NORMAL = 15f;
        private const float SECTION_SPACING = 4f;

        // チェックボックス3つ + ラジオボタン6つ + セパレータ + 単位選択エリア
        private const int CHECKBOX_COUNT = 3;
        private const int RADIO_BUTTON_COUNT = 6;
        private float MENU_CONTENT_HEIGHT => (CHECKBOX_COUNT * LINE_HEIGHT_NORMAL) + 
                                              SECTION_SPACING + 
                                              (RADIO_BUTTON_COUNT * LINE_HEIGHT_NORMAL);

        // 単位選択エリアの定数
        private const float UNIT_DROPDOWN_HEIGHT = 16f;
        private const float UNIT_SPACING = 2f;
        private const float UNIT_AREAHEIGHT = UNIT_DROPDOWN_HEIGHT + UNIT_SPACING;
        
        private static readonly Color CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color TAB_TEXT_COLOR = Color.White;
        private static readonly Color GROUP_BACKGROUND_COLOR = Color.FromArgb(240, 240, 240);

        #endregion

        #region Fields

        private SectionForceView Cmp => base.Owner as SectionForceView;

        private RectangleF sectionForcesArea;
        
        // チェックボックスグループ
        private RectangleF checkboxGroupArea;
        private RectangleF filledCheckBox;
        private RectangleF numbersCheckBox;
        private RectangleF legendCheckBox;
        
        // ラジオボタングループ
        private RectangleF radioGroupArea;
        private RectangleF fxRadio;
        private RectangleF fyRadio;
        private RectangleF fzRadio;
        private RectangleF mxRadio;
        private RectangleF myRadio;
        private RectangleF mzRadio;

        // 単位選択用のフィールド
        private RectangleF _forceUnitDropdownBounds;
        private RectangleF _lengthUnitDropdownBounds;

        #endregion

        public SectionForceViewAttributes(SectionForceView owner) : base(owner) { }

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);

            float extraHeight = TAB_HEIGHT + TAB_MARGIN_TOP + TAB_MARGIN_BOTTOM;
            if (Cmp.IsSectionForcesTabExpanded)
            {
                extraHeight += MENU_CONTENT_HEIGHT;
                extraHeight += UNIT_AREAHEIGHT;
                extraHeight += COMPONENT_MARGIN_VERTICAL * 2;
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
                float rightPosition = bounds.Right - CONTROL_RIGHT_MARGIN;

                // チェックボックスグループ
                float checkboxGroupTop = currentY;
                
                filledCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                numbersCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                legendCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                checkboxGroupArea = new RectangleF(
                    bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                    checkboxGroupTop - 2,
                    bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                    (CHECKBOX_COUNT * LINE_HEIGHT_NORMAL) + 2
                );

                // セクション区切り
                currentY += SECTION_SPACING;

                // ラジオボタングループ
                float radioGroupTop = currentY;

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
                currentY += LINE_HEIGHT_NORMAL;

                radioGroupArea = new RectangleF(
                    bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                    radioGroupTop - 2,
                     bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                    (RADIO_BUTTON_COUNT * LINE_HEIGHT_NORMAL) + 2
                );
                
                // 単位選択エリアのレイアウト
                float unitStartY = radioGroupArea.Bottom+ UNIT_SPACING;
                float centerX = bounds.Left + bounds.Width / 2;
                
                float dropdownWidth = (bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2)) / 2 - UNIT_SPACING;
                float totalWidth = dropdownWidth * 2 + UNIT_SPACING;
                float leftX = centerX - totalWidth / 2;
                
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

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                RenderTab(graphics);

                if (Cmp.IsSectionForcesTabExpanded)
                {
                    RenderMenuContent(graphics);
                    RenderUnitSelection(graphics);
                }
            }
        }

        private void RenderTab(Graphics graphics)
        {
            GH_Palette palette = GH_Palette.Black;

            GH_Capsule tabCapsule = GH_Capsule.CreateCapsule(sectionForcesArea, palette, 2, 4);
            tabCapsule.Render(graphics, Selected, Cmp.Locked, false);
            tabCapsule.Dispose();

            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            
            using (SolidBrush textBrush = new SolidBrush(TAB_TEXT_COLOR))
            {
                graphics.DrawString("Section Forces", GH_FontServer.StandardBold, textBrush, sectionForcesArea, format);
            }
        }

        private void RenderMenuContent(Graphics graphics)
        {
            var font = GH_FontServer.Small;
            float leftMargin = Bounds.Left + MENU_LEFT_MARGIN;

            // チェックボックスグループのカプセル
            GH_Palette palette = GH_Palette.White;
            if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error)
                palette = GH_Palette.Error;
            else if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning)
                palette = GH_Palette.Warning;

            GH_Capsule checkboxCapsule = GH_Capsule.CreateCapsule(checkboxGroupArea, palette, 2, 0);
            checkboxCapsule.Render(graphics, Selected, Cmp.Locked,Cmp.Hidden);
            checkboxCapsule.Dispose();

            // ラジオボタングループのカプセル
            GH_Capsule radioCapsule = GH_Capsule.CreateCapsule(radioGroupArea, palette, 2, 0);
            radioCapsule.Render(graphics, Selected, Cmp.Locked, Cmp.Hidden);
            radioCapsule.Dispose();

            using (SolidBrush textBrush = new SolidBrush(TEXT_COLOR))
            {
                // チェックボックス
                graphics.DrawString("Filled", font, textBrush, leftMargin, filledCheckBox.Top);
                DrawCheckBox(graphics, filledCheckBox, Cmp.ShowFilled);

                graphics.DrawString("Numbers", font, textBrush, leftMargin, numbersCheckBox.Top);
                DrawCheckBox(graphics, numbersCheckBox, Cmp.ShowNumbers);

                graphics.DrawString("Legend", font, textBrush, leftMargin, legendCheckBox.Top);
                DrawCheckBox(graphics, legendCheckBox, Cmp.ShowLegend);

                // ラジオボタン
                graphics.DrawString("Fx", font, textBrush, leftMargin, fxRadio.Top);
                DrawRadioButton(graphics, fxRadio, Cmp.SelectedForceType == SectionForceView.SectionForceType.Fx);

                graphics.DrawString("Fy", font, textBrush, leftMargin, fyRadio.Top);
                DrawRadioButton(graphics, fyRadio, Cmp.SelectedForceType == SectionForceView.SectionForceType.Fy);

                graphics.DrawString("Fz", font, textBrush, leftMargin, fzRadio.Top);
                DrawRadioButton(graphics, fzRadio, Cmp.SelectedForceType == SectionForceView.SectionForceType.Fz);

                graphics.DrawString("Mx", font, textBrush, leftMargin, mxRadio.Top);
                DrawRadioButton(graphics, mxRadio, Cmp.SelectedForceType == SectionForceView.SectionForceType.Mx);

                graphics.DrawString("My", font, textBrush, leftMargin, myRadio.Top);
                DrawRadioButton(graphics, myRadio, Cmp.SelectedForceType == SectionForceView.SectionForceType.My);

                graphics.DrawString("Mz", font, textBrush, leftMargin, mzRadio.Top);
                DrawRadioButton(graphics, mzRadio, Cmp.SelectedForceType == SectionForceView.SectionForceType.Mz);
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
            graphics.DrawEllipse(Pens.Black, rect);

            if (isSelected)
            {
                RectangleF innerRect = new RectangleF(
                    rect.X + CONTROL_FILL_MARGIN,
                    rect.Y + CONTROL_FILL_MARGIN,
                    rect.Width - (CONTROL_FILL_MARGIN * 2),
                    rect.Height - (CONTROL_FILL_MARGIN * 2)
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
                
                if (sectionForcesArea.Contains(e.CanvasLocation))
                {
                    Cmp.IsSectionForcesTabExpanded = !Cmp.IsSectionForcesTabExpanded;
                    Cmp.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }

                if (Cmp.IsSectionForcesTabExpanded)
                {
                    if (filledCheckBox.Contains(e.CanvasLocation))
                    {
                        Cmp.ShowFilled = !Cmp.ShowFilled;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (numbersCheckBox.Contains(e.CanvasLocation))
                    {
                        Cmp.ShowNumbers = !Cmp.ShowNumbers;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

                    if (legendCheckBox.Contains(e.CanvasLocation))
                    {
                        Cmp.ShowLegend = !Cmp.ShowLegend;
                        Cmp.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }

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
                    Cmp.ExpireSolution(true);
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
                    Cmp.ExpireSolution(true);
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
    }
}