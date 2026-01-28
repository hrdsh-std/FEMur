using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace FEMurGH.Comoponents.Results
{
    /// <summary>
    /// ドロップダウンUI描画用のヘルパークラス
    /// </summary>
    public static class DropdownRenderer
    {
        private const float DEFAULT_CORNER_RADIUS = 4f;
        private const float DEFAULT_INFLATE = -2f;
        private const float TEXT_MARGIN_LEFT = 2f;
        private const float TEXT_MARGIN_RIGHT = 12f;
        private const float ARROW_SIZE = 4f;

        /// <summary>
        /// ドロップダウンUIを描画
        /// </summary>
        /// <param name="graphics">描画先のGraphicsオブジェクト</param>
        /// <param name="bounds">描画領域</param>
        /// <param name="text">表示するテキスト</param>
        /// <param name="component">コンポーネント（色の取得に使用）</param>
        /// <param name="selected">選択状態</param>
        /// <param name="cornerRadius">角の丸み（デフォルト: 4f）</param>
        public static void DrawDropdown(
            Graphics graphics, 
            RectangleF bounds, 
            string text, 
            IGH_ActiveObject component,
            bool selected,
            float cornerRadius = DEFAULT_CORNER_RADIUS)
        {
            // コンポーネントの状態に応じたパレットを取得
            GH_Palette palette = GH_CapsuleRenderEngine.GetImpliedPalette(component);

            // ドロップダウン個別のカプセル
            GH_Capsule capsule = GH_Capsule.CreateCapsule(bounds, palette, (int)cornerRadius, 0);
            capsule.Render(graphics, selected, component.Locked,false);
            capsule.Dispose();

            // 内側の領域を計算
            RectangleF innerBounds = bounds;
            innerBounds.Inflate(DEFAULT_INFLATE, DEFAULT_INFLATE);

            // ドロップダウンの背景（角丸の矩形）
            using (GraphicsPath path = CreateRoundedRectanglePath(innerBounds, cornerRadius + DEFAULT_INFLATE))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillPath(Brushes.White, path);
                graphics.SmoothingMode = SmoothingMode.Default;
            }

            // テキストの描画
            DrawText(graphics, innerBounds, text);

            // ドロップダウン矢印の描画
            DrawArrow(graphics, innerBounds);
        }

        /// <summary>
        /// 角丸矩形のGraphicsPathを作成
        /// </summary>
        private static GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2f;

            // 左上の角
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // 右上の角
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // 右下の角
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // 左下の角
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// テキストを描画
        /// </summary>
        private static void DrawText(Graphics graphics, RectangleF bounds, string text)
        {
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            RectangleF textBounds = new RectangleF(
                bounds.X + TEXT_MARGIN_LEFT, 
                bounds.Y, 
                bounds.Width - TEXT_MARGIN_RIGHT, 
                bounds.Height);

            graphics.DrawString(text, GH_FontServer.Small, Brushes.Black, textBounds, format);
        }

        /// <summary>
        /// ドロップダウン矢印を描画
        /// </summary>
        private static void DrawArrow(Graphics graphics, RectangleF bounds)
        {
            PointF[] arrow = new PointF[]
            {
                new PointF(bounds.Right - ARROW_SIZE * 2, bounds.Y + bounds.Height / 2 - ARROW_SIZE / 2),
                new PointF(bounds.Right - ARROW_SIZE, bounds.Y + bounds.Height / 2 - ARROW_SIZE / 2),
                new PointF(bounds.Right - ARROW_SIZE * 1.5f, bounds.Y + bounds.Height / 2 + ARROW_SIZE / 2)
            };
            graphics.FillPolygon(Brushes.Black, arrow);
        }
    }
}