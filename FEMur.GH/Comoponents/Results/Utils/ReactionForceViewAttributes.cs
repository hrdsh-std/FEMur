using System;
using System.Drawing;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// ReactionForceView コンポーネントのカスタムUI属性
    /// Supportコンポーネントと同じデザインスタイル
    /// </summary>
    public class ReactionForceViewAttributes : GH_ComponentAttributes
    {
        #region UI Layout Constants (Supportコンポーネントと同じ)

        private const float COMPONENT_MARGIN_HORIZONTAL = 2f;
        private const float TAB_HEIGHT = 14f;
        private const float TAB_MARGIN_TOP = 4f;
        private const float TAB_MARGIN_BOTTOM = 2f;

        private const float MENU_LEFT_MARGIN = 8f;
        private const float MENU_TOP_PADDING = 2f;
        private const float MENU_BOTTOM_PADDING = 2f;

        private const float RADIO_BUTTON_SIZE = 10f;
        private const float RADIO_BUTTON_SPACING = 26f; // より詰める
        private const float LINE_HEIGHT = 15f;
        private const float LABEL_HEIGHT = 12f;

        private const int CHECKBOX_COUNT = 6; // Fx, Fy, Fz, Mx, My, Mz
        private float MENU_CONTENT_HEIGHT => LABEL_HEIGHT + LINE_HEIGHT + MENU_TOP_PADDING + MENU_BOTTOM_PADDING;

        private static readonly Color CONTROL_FILL_COLOR = Color.FromArgb(80, 80, 80);
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color TAB_TEXT_COLOR = Color.White;

        #endregion

        #region Fields

        private ReactionForceView Cmp => base.Owner as ReactionForceView;

        private RectangleF reactionsArea;
        private RectangleF menuContentArea;
        private RectangleF fxRadio;
        private RectangleF fyRadio;
        private RectangleF fzRadio;
        private RectangleF mxRadio;
        private RectangleF myRadio;
        private RectangleF mzRadio;

        #endregion

        public ReactionForceViewAttributes(ReactionForceView owner) : base(owner) { }

        #region Layout Methods

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);

            // コンポーネントの幅は変更しない

            float extraHeight = TAB_HEIGHT + TAB_MARGIN_TOP + TAB_MARGIN_BOTTOM;
            if (Cmp.IsReactionsTabExpanded)
            {
                extraHeight += MENU_CONTENT_HEIGHT;
            }

            bounds.Height += extraHeight;
            Bounds = bounds;

            float tabY = bounds.Bottom - extraHeight + TAB_MARGIN_TOP;
            reactionsArea = new RectangleF(
                bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                tabY,
                bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                TAB_HEIGHT
            );

            if (Cmp.IsReactionsTabExpanded)
            {
                float menuTop = reactionsArea.Bottom;
                menuContentArea = new RectangleF(
                    bounds.Left + COMPONENT_MARGIN_HORIZONTAL,
                    menuTop,
                    bounds.Width - (COMPONENT_MARGIN_HORIZONTAL * 2),
                    MENU_CONTENT_HEIGHT
                );

                float currentY = menuContentArea.Top + MENU_TOP_PADDING + LABEL_HEIGHT;
                
                // 6つのラジオボタンの総幅を計算
                float totalRadioWidth = (CHECKBOX_COUNT - 1) * RADIO_BUTTON_SPACING + RADIO_BUTTON_SIZE;
                
                // 中央揃え：メニューの中央から総幅の半分を引いた位置を開始点とする
                float startX = menuContentArea.Left + (menuContentArea.Width - totalRadioWidth) / 2;

                // ラジオボタンを横並びに配置（中央揃え）
                fxRadio = new RectangleF(startX, currentY, RADIO_BUTTON_SIZE, RADIO_BUTTON_SIZE);
                fyRadio = new RectangleF(startX + RADIO_BUTTON_SPACING, currentY, RADIO_BUTTON_SIZE, RADIO_BUTTON_SIZE);
                fzRadio = new RectangleF(startX + RADIO_BUTTON_SPACING * 2, currentY, RADIO_BUTTON_SIZE, RADIO_BUTTON_SIZE);
                mxRadio = new RectangleF(startX + RADIO_BUTTON_SPACING * 3, currentY, RADIO_BUTTON_SIZE, RADIO_BUTTON_SIZE);
                myRadio = new RectangleF(startX + RADIO_BUTTON_SPACING * 4, currentY, RADIO_BUTTON_SIZE, RADIO_BUTTON_SIZE);
                mzRadio = new RectangleF(startX + RADIO_BUTTON_SPACING * 5, currentY, RADIO_BUTTON_SIZE, RADIO_BUTTON_SIZE);
            }
        }

        #endregion

        #region Rendering Methods

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                RenderTab(graphics);

                if (Cmp.IsReactionsTabExpanded)
                {
                    RenderMenuContent(graphics);
                }
            }
        }

        /// <summary>
        /// Reactionsタブを描画
        /// </summary>
        private void RenderTab(Graphics graphics)
        {
            GH_Palette palette = GH_Palette.Black;

            // タブ全体のカプセルを描画
            GH_Capsule tabCapsule = GH_Capsule.CreateCapsule(reactionsArea, palette, 2, 4);
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
                graphics.DrawString("Reactions", GH_FontServer.StandardBold, textBrush, reactionsArea, format);
            }
        }

        /// <summary>
        /// メニューコンテンツを描画
        /// </summary>
        private void RenderMenuContent(Graphics graphics)
        {
            // メニュー背景のカプセルを描画（Supportコンポーネントと同様）
            GH_Palette palette = GH_Palette.White;
            if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error)
                palette = GH_Palette.Error;
            else if (Cmp.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning)
                palette = GH_Palette.Warning;
            GH_Capsule menuCapsule = GH_Capsule.CreateCapsule(menuContentArea, palette, 2, 0);
            menuCapsule.Render(graphics, Selected, Cmp.Locked, false);
            menuCapsule.Dispose();

            var font = GH_FontServer.Small;

            using (SolidBrush textBrush = new SolidBrush(TEXT_COLOR))
            {
                float labelY = menuContentArea.Top + MENU_TOP_PADDING;

                // ラベルをラジオボタンの中央に配置
                StringFormat centerFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };

                // Fx
                RectangleF fxLabelRect = new RectangleF(fxRadio.X - 5, labelY, RADIO_BUTTON_SIZE + 10, LABEL_HEIGHT);
                graphics.DrawString("Fx", font, textBrush, fxLabelRect, centerFormat);
                DrawRadioButton(graphics, fxRadio, Cmp.ShowFx);

                // Fy
                RectangleF fyLabelRect = new RectangleF(fyRadio.X - 5, labelY, RADIO_BUTTON_SIZE + 10, LABEL_HEIGHT);
                graphics.DrawString("Fy", font, textBrush, fyLabelRect, centerFormat);
                DrawRadioButton(graphics, fyRadio, Cmp.ShowFy);

                // Fz
                RectangleF fzLabelRect = new RectangleF(fzRadio.X - 5, labelY, RADIO_BUTTON_SIZE + 10, LABEL_HEIGHT);
                graphics.DrawString("Fz", font, textBrush, fzLabelRect, centerFormat);
                DrawRadioButton(graphics, fzRadio, Cmp.ShowFz);

                // Mx
                RectangleF mxLabelRect = new RectangleF(mxRadio.X - 5, labelY, RADIO_BUTTON_SIZE + 10, LABEL_HEIGHT);
                graphics.DrawString("Mx", font, textBrush, mxLabelRect, centerFormat);
                DrawRadioButton(graphics, mxRadio, Cmp.ShowMx);

                // My
                RectangleF myLabelRect = new RectangleF(myRadio.X - 5, labelY, RADIO_BUTTON_SIZE + 10, LABEL_HEIGHT);
                graphics.DrawString("My", font, textBrush, myLabelRect, centerFormat);
                DrawRadioButton(graphics, myRadio, Cmp.ShowMy);

                // Mz
                RectangleF mzLabelRect = new RectangleF(mzRadio.X - 5, labelY, RADIO_BUTTON_SIZE + 10, LABEL_HEIGHT);
                graphics.DrawString("Mz", font, textBrush, mzLabelRect, centerFormat);
                DrawRadioButton(graphics, mzRadio, Cmp.ShowMz);
            }
        }

        /// <summary>
        /// ラジオボタンを描画（○形式、Supportと同じ）
        /// </summary>
        private void DrawRadioButton(Graphics graphics, RectangleF rect, bool isChecked)
        {
            // 外側の円
            graphics.DrawEllipse(Pens.Black, rect);

            // 選択されている場合は内側を塗りつぶす
            if (isChecked)
            {
                const float margin = 2f;
                RectangleF innerRect = new RectangleF(
                    rect.X + margin,
                    rect.Y + margin,
                    rect.Width - (margin * 2),
                    rect.Height - (margin * 2)
                );
                using (SolidBrush fillBrush = new SolidBrush(CONTROL_FILL_COLOR))
                {
                    graphics.FillEllipse(fillBrush, innerRect);
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
                if (reactionsArea.Contains(e.CanvasLocation))
                {
                    Cmp.IsReactionsTabExpanded = !Cmp.IsReactionsTabExpanded;
                    Cmp.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }

                // 展開時のみラジオボタン処理
                if (Cmp.IsReactionsTabExpanded)
                {
                    if (HandleRadioButtonClick(fxRadio, e.CanvasLocation, 
                        () => Cmp.ShowFx, v => Cmp.ShowFx = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleRadioButtonClick(fyRadio, e.CanvasLocation, 
                        () => Cmp.ShowFy, v => Cmp.ShowFy = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleRadioButtonClick(fzRadio, e.CanvasLocation, 
                        () => Cmp.ShowFz, v => Cmp.ShowFz = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleRadioButtonClick(mxRadio, e.CanvasLocation, 
                        () => Cmp.ShowMx, v => Cmp.ShowMx = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleRadioButtonClick(myRadio, e.CanvasLocation, 
                        () => Cmp.ShowMy, v => Cmp.ShowMy = v))
                        return GH_ObjectResponse.Handled;

                    if (HandleRadioButtonClick(mzRadio, e.CanvasLocation, 
                        () => Cmp.ShowMz, v => Cmp.ShowMz = v))
                        return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }

        /// <summary>
        /// ラジオボタンのクリック処理（複数選択可能）
        /// </summary>
        private bool HandleRadioButtonClick(RectangleF rect, PointF location, 
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