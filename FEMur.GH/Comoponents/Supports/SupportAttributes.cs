using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace FEMurGH.Comoponents.Supports
{
    /// <summary>
    /// Support コンポーネントのカスタム UI 属性
    /// コンポーネント下部に拘束条件の選択UIを表示
    /// </summary>
    public class SupportAttributes : GH_ComponentAttributes
    {
        private const float CHECKBOX_SIZE = 9f;
        private const float ITEM_WIDTH = 18f;  // ラベル+チェックボックスの幅
        private const float SPACING = 2f;      // アイテム間のスペース
        private const float UI_HEIGHT = 36f;
        private const float MARGIN_TOP = 3f;
        private const float MARGIN_HORIZONTAL = 5f; // 左右のマージン
        private const float CORNER_RADIUS = 4f;    // 角の丸み

        private Support Cmp => base.Owner as Support;

        private RectangleF uiArea;
        private RectangleF uxCheckBox;
        private RectangleF uyCheckBox;
        private RectangleF uzCheckBox;
        private RectangleF rxCheckBox;
        private RectangleF ryCheckBox;
        private RectangleF rzCheckBox;

        public SupportAttributes(Support owner) : base(owner) { }

        protected override void Layout()
        {
            base.Layout();

            RectangleF bounds = GH_Convert.ToRectangle(Bounds);
            bounds.Height += UI_HEIGHT;
            Bounds = bounds;

            // UI エリアの定義（コンポーネントの境界から一定のマージンを設定）
            float uiTop = bounds.Bottom - UI_HEIGHT + MARGIN_TOP;
            uiArea = new RectangleF(
                bounds.Left + MARGIN_HORIZONTAL - 3, 
                uiTop, 
                bounds.Width - ((MARGIN_HORIZONTAL - 3) * 2), 
                UI_HEIGHT - MARGIN_TOP - 2f
            );

            // チェックボックスの配置（コンパクトに）
            float totalWidth = ITEM_WIDTH * 6 + SPACING * 5;
            float startX = uiArea.Left + (uiArea.Width - totalWidth) / 2;
            float checkY = uiArea.Top + 18f;

            uxCheckBox = new RectangleF(startX + (ITEM_WIDTH - CHECKBOX_SIZE) / 2, checkY, CHECKBOX_SIZE, CHECKBOX_SIZE);
            startX += ITEM_WIDTH + SPACING;

            uyCheckBox = new RectangleF(startX + (ITEM_WIDTH - CHECKBOX_SIZE) / 2, checkY, CHECKBOX_SIZE, CHECKBOX_SIZE);
            startX += ITEM_WIDTH + SPACING;

            uzCheckBox = new RectangleF(startX + (ITEM_WIDTH - CHECKBOX_SIZE) / 2, checkY, CHECKBOX_SIZE, CHECKBOX_SIZE);
            startX += ITEM_WIDTH + SPACING;

            rxCheckBox = new RectangleF(startX + (ITEM_WIDTH - CHECKBOX_SIZE) / 2, checkY, CHECKBOX_SIZE, CHECKBOX_SIZE);
            startX += ITEM_WIDTH + SPACING;

            ryCheckBox = new RectangleF(startX + (ITEM_WIDTH - CHECKBOX_SIZE) / 2, checkY, CHECKBOX_SIZE, CHECKBOX_SIZE);
            startX += ITEM_WIDTH + SPACING;

            rzCheckBox = new RectangleF(startX + (ITEM_WIDTH - CHECKBOX_SIZE) / 2, checkY, CHECKBOX_SIZE, CHECKBOX_SIZE);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                RenderUI(graphics);
            }
        }

        private void RenderUI(Graphics graphics)
        {
            // アンチエイリアスを有効にして滑らかな描画
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // GH_Capsule を使用して UI エリアを描画
            GH_Capsule capsule = GH_Capsule.CreateCapsule(uiArea, GH_Palette.Transparent, 2, 5);
            capsule.Render(graphics, Selected, Cmp.Locked, false);
            capsule.Dispose();

            var font = GH_FontServer.Small;
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                StringFormat centerFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // ラベルの描画（各チェックボックスの中心に合わせる）
                float labelY = uiArea.Top + 7f;
                
                float uxCenterX = uxCheckBox.X + CHECKBOX_SIZE / 2;
                float uyCenterX = uyCheckBox.X + CHECKBOX_SIZE / 2;
                float uzCenterX = uzCheckBox.X + CHECKBOX_SIZE / 2;
                float rxCenterX = rxCheckBox.X + CHECKBOX_SIZE / 2;
                float ryCenterX = ryCheckBox.X + CHECKBOX_SIZE / 2;
                float rzCenterX = rzCheckBox.X + CHECKBOX_SIZE / 2;

                graphics.DrawString("Ux", font, textBrush, uxCenterX, labelY, centerFormat);
                graphics.DrawString("Uy", font, textBrush, uyCenterX, labelY, centerFormat);
                graphics.DrawString("Uz", font, textBrush, uzCenterX, labelY, centerFormat);
                graphics.DrawString("Rx", font, textBrush, rxCenterX, labelY, centerFormat);
                graphics.DrawString("Ry", font, textBrush, ryCenterX, labelY, centerFormat);
                graphics.DrawString("Rz", font, textBrush, rzCenterX, labelY, centerFormat);

                // チェックボックスの描画
                DrawCheckBox(graphics, uxCheckBox, Cmp.UX);
                DrawCheckBox(graphics, uyCheckBox, Cmp.UY);
                DrawCheckBox(graphics, uzCheckBox, Cmp.UZ);
                DrawCheckBox(graphics, rxCheckBox, Cmp.RX);
                DrawCheckBox(graphics, ryCheckBox, Cmp.RY);
                DrawCheckBox(graphics, rzCheckBox, Cmp.RZ);
            }

            // SmoothingMode を元に戻す
            graphics.SmoothingMode = SmoothingMode.Default;
        }

        private void DrawCheckBox(Graphics graphics, RectangleF rect, bool isChecked)
        {
            // チェックボックスの外枠（円）
            using (Pen pen = new Pen(Color.Black, 1))
            {
                graphics.DrawEllipse(pen, rect);
            }

            // チェック状態の場合は塗りつぶし
            if (isChecked)
            {
                RectangleF fillRect = new RectangleF(
                    rect.X + 2,
                    rect.Y + 2,
                    rect.Width - 4,
                    rect.Height - 4
                );
                using (SolidBrush fillBrush = new SolidBrush(Color.Black))
                {
                    graphics.FillEllipse(fillBrush, fillRect);
                }
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (uxCheckBox.Contains(e.CanvasLocation))
                {
                    Cmp.ToggleUX();
                    return GH_ObjectResponse.Handled;
                }
                if (uyCheckBox.Contains(e.CanvasLocation))
                {
                    Cmp.ToggleUY();
                    return GH_ObjectResponse.Handled;
                }
                if (uzCheckBox.Contains(e.CanvasLocation))
                {
                    Cmp.ToggleUZ();
                    return GH_ObjectResponse.Handled;
                }
                if (rxCheckBox.Contains(e.CanvasLocation))
                {
                    Cmp.ToggleRX();
                    return GH_ObjectResponse.Handled;
                }
                if (ryCheckBox.Contains(e.CanvasLocation))
                {
                    Cmp.ToggleRY();
                    return GH_ObjectResponse.Handled;
                }
                if (rzCheckBox.Contains(e.CanvasLocation))
                {
                    Cmp.ToggleRZ();
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
}