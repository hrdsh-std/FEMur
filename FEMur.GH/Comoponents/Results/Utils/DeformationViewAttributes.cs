using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// DeformationView コンポーネントのカスタムUI属性
    /// </summary>
    public class DeformationViewAttributes : GH_ComponentAttributes
    {
        #region UI Layout Constants

        private const float COMPONENT_MARGIN_HORIZONTAL = 2f;
        private const float COMPONENT_MARGIN_VERTICAL = 4f;

        private const float TAB_HEIGHT = 14f;
        private const float TAB_MARGIN_TOP = 4f;
        private const float TAB_MARGIN_BOTTOM = 2f;

        private const float MENU_LEFT_MARGIN = 8f;
        private const float MENU_TOP_PADDING = 5f;
        private const float MENU_BOTTOM_PADDING = 3f;

        private const float CONTROL_SIZE = 10f;
        private const float CONTROL_RIGHT_MARGIN = 15f;
        private const float CONTROL_FILL_MARGIN = 2f;
        private const float RADIO_BUTTON_SIZE = 10f;

        private const float LINE_HEIGHT_NORMAL = 15f;
        private const float SECTION_SPACING = 8f;

        // ラジオボタン7つ + セパレータ + チェックボックス2つ
        private const int RADIO_BUTTON_COUNT = 7;
        private const int CHECKBOX_COUNT = 2;
        private float MENU_CONTENT_HEIGHT => (RADIO_BUTTON_COUNT * LINE_HEIGHT_NORMAL) + 
                                              SECTION_SPACING + 
                                              (CHECKBOX_COUNT * LINE_HEIGHT_NORMAL) + 
                                              MENU_TOP_PADDING + 
                                              MENU_BOTTOM_PADDING;

        private static readonly Color TAB_BACKGROUND_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color TAB_TEXT_COLOR = Color.White;

        #endregion

        #region Fields

        private DeformationView Cmp => base.Owner as DeformationView;

        private RectangleF displayArea;
        
        // ラジオボタン
        private RectangleF dxRadio;
        private RectangleF dyRadio;
        private RectangleF dzRadio;
        private RectangleF dxyRadio;
        private RectangleF dyzRadio;
        private RectangleF dzxRadio;
        private RectangleF dxyzRadio;

        // チェックボックス
        private RectangleF showNumbersCheckBox;
        private RectangleF showLegendCheckBox;

        #endregion

        public DeformationViewAttributes(DeformationView owner) : base(owner) { }

        #region Layout Methods

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);

            float extraHeight = TAB_HEIGHT + TAB_MARGIN_TOP + TAB_MARGIN_BOTTOM;
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

                // ラジオボタン（変形方向選択）
                dxRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                dyRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                dzRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                dxyRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                dyzRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                dzxRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                dxyzRadio = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                // セクション区切り
                currentY += SECTION_SPACING;

                // チェックボックス
                showNumbersCheckBox = CreateControlRect(rightPosition, currentY);
                currentY += LINE_HEIGHT_NORMAL;

                showLegendCheckBox = CreateControlRect(rightPosition, currentY);
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

            using (SolidBrush textBrush = new SolidBrush(TEXT_COLOR))
            {
                // ラジオボタン（変形方向選択）
                graphics.DrawString("Dx", font, textBrush, leftMargin, dxRadio.Top);
                DrawRadioButton(graphics, dxRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dx);

                graphics.DrawString("Dy", font, textBrush, leftMargin, dyRadio.Top);
                DrawRadioButton(graphics, dyRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dy);

                graphics.DrawString("Dz", font, textBrush, leftMargin, dzRadio.Top);
                DrawRadioButton(graphics, dzRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dz);

                graphics.DrawString("Dxy", font, textBrush, leftMargin, dxyRadio.Top);
                DrawRadioButton(graphics, dxyRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dxy);

                graphics.DrawString("Dyz", font, textBrush, leftMargin, dyzRadio.Top);
                DrawRadioButton(graphics, dyzRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dyz);

                graphics.DrawString("Dzx", font, textBrush, leftMargin, dzxRadio.Top);
                DrawRadioButton(graphics, dzxRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dzx);

                graphics.DrawString("Dxyz", font, textBrush, leftMargin, dxyzRadio.Top);
                DrawRadioButton(graphics, dxyzRadio, Cmp.SelectedDirection == DeformationView.DeformationDirection.Dxyz);

                // チェックボックス
                graphics.DrawString("ShowNumbers", font, textBrush, leftMargin, showNumbersCheckBox.Top);
                DrawCheckBox(graphics, showNumbersCheckBox, Cmp.ShowNumbers);

                graphics.DrawString("Legend", font, textBrush, leftMargin, showLegendCheckBox.Top);
                DrawCheckBox(graphics, showLegendCheckBox, Cmp.ShowLegend);
            }
        }

        /// <summary>
        /// ラジオボタンを描画
        /// </summary>
        private void DrawRadioButton(Graphics graphics, RectangleF rect, bool isSelected)
        {
            // 円形の外枠
            graphics.DrawEllipse(Pens.Black, rect);

            // 選択されている場合は内側を塗りつぶす
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

                // 展開時のみコントロール処理
                if (Cmp.IsDisplayTabExpanded)
                {
                    // ラジオボタン（変形方向選択）
                    if (HandleRadioButtonClick(dxRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dx))
                        return GH_ObjectResponse.Handled;
                    if (HandleRadioButtonClick(dyRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dy))
                        return GH_ObjectResponse.Handled;
                    if (HandleRadioButtonClick(dzRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dz))
                        return GH_ObjectResponse.Handled;
                    if (HandleRadioButtonClick(dxyRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dxy))
                        return GH_ObjectResponse.Handled;
                    if (HandleRadioButtonClick(dyzRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dyz))
                        return GH_ObjectResponse.Handled;
                    if (HandleRadioButtonClick(dzxRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dzx))
                        return GH_ObjectResponse.Handled;
                    if (HandleRadioButtonClick(dxyzRadio, e.CanvasLocation, DeformationView.DeformationDirection.Dxyz))
                        return GH_ObjectResponse.Handled;

                    // チェックボックス
                    if (HandleCheckBoxClick(showNumbersCheckBox, e.CanvasLocation, 
                        () => Cmp.ShowNumbers, v => Cmp.ShowNumbers = v))
                        return GH_ObjectResponse.Handled;
                    if (HandleCheckBoxClick(showLegendCheckBox, e.CanvasLocation, 
                        () => Cmp.ShowLegend, v => Cmp.ShowLegend = v))
                        return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }

        /// <summary>
        /// ラジオボタンのクリック処理
        /// </summary>
        private bool HandleRadioButtonClick(RectangleF rect, PointF location, DeformationView.DeformationDirection direction)
        {
            if (rect.Contains(location))
            {
                Cmp.SelectedDirection = direction;
                Cmp.ExpireSolution(true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// チェックボックスのクリック処理
        /// </summary>
        private bool HandleCheckBoxClick(RectangleF rect, PointF location, 
            Func<bool> getter, Action<bool> setter)
        {
            if (rect.Contains(location))
            {
                setter(!getter());
                Cmp.ExpireSolution(true);
                return true;
            }
            return false;
        }

        #endregion
    }
}