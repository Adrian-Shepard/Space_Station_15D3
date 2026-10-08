using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.NTUI
{
    public static class InteractionMenuMetrics
    {
        public const float ItemWidth = 240f;
        public const float ItemHeight = 22f;
        public const float Padding = 6f;
        public const float TextPadX = 8f;
        public const float TextScale = 0.45f;

        // Отступ от центра экрана по X (чтобы не накрывать крестик)
        public const float CenterOffsetX = 50f;
    }

    public class InteractionMenuRenderer
    {
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        public InteractionMenuRenderer(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        /// <summary>
        /// Фиксированное положение панели: справа от центра экрана, по центру по вертикали.
        /// Не зависит от позиции мыши.
        /// </summary>
        public static void ComputePanelRect(
            int itemCount, float screenWidth, float screenHeight,
            out float panelX, out float panelY,
            out float panelW, out float panelH)
        {
            panelW = InteractionMenuMetrics.ItemWidth + InteractionMenuMetrics.Padding * 2f;
            panelH = itemCount * InteractionMenuMetrics.ItemHeight + InteractionMenuMetrics.Padding * 2f;

            // По центру справа от крестика
            panelX = screenWidth / 2f + InteractionMenuMetrics.CenterOffsetX;
            panelY = (screenHeight - panelH) / 2f;

            // Отступ от краёв
            const float edge = 8f;
            if (panelX + panelW > screenWidth - edge)
                panelX = screenWidth - panelW - edge;
            if (panelX < edge)
                panelX = edge;
            if (panelY < edge)
                panelY = edge;
            if (panelY + panelH > screenHeight - edge)
                panelY = screenHeight - panelH - edge;
        }

        public void Draw(SpriteBatch sb, InteractionMenu menu,
            float screenWidth, float screenHeight)
        {
            if (!menu.IsOpen) return;

            int rows = menu.Items.Count + (string.IsNullOrEmpty(menu.Title) ? 0 : 1);

            ComputePanelRect(rows, screenWidth, screenHeight,
                out float x, out float y, out float w, out float h);

            // Фон панели
            sb.Draw(_bg, new Vector2(x, y), null,
                new Vector2(w, h), 0, Vector2.Zero,
                new Color(0f, 0f, 0f, 0.85f));

            // Рамка
            var borderCol = new Color(0.55f, 0.6f, 0.75f, 0.9f);
            sb.Draw(_bg, new Vector2(x, y), null, new Vector2(w, 1f), 0, Vector2.Zero, borderCol);
            sb.Draw(_bg, new Vector2(x, y + h - 1f), null, new Vector2(w, 1f), 0, Vector2.Zero, borderCol);
            sb.Draw(_bg, new Vector2(x, y), null, new Vector2(1f, h), 0, Vector2.Zero, borderCol);
            sb.Draw(_bg, new Vector2(x + w - 1f, y), null, new Vector2(1f, h), 0, Vector2.Zero, borderCol);

            float curY = y + InteractionMenuMetrics.Padding;

            // Заголовок
            if (!string.IsNullOrEmpty(menu.Title))
            {
                _font.DrawText(sb, menu.Title,
                    new Vector2(x + InteractionMenuMetrics.Padding + InteractionMenuMetrics.TextPadX,
                                curY + 3f),
                    InteractionMenuMetrics.TextScale,
                    new Color(1f, 0.9f, 0.55f, 1f));
                curY += InteractionMenuMetrics.ItemHeight;

                sb.Draw(_bg, new Vector2(x + 2f, curY), null,
                    new Vector2(w - 4f, 1f), 0, Vector2.Zero,
                    new Color(0.4f, 0.45f, 0.55f, 0.7f));
            }

            // Пункты
            for (int i = 0; i < menu.Items.Count; i++)
            {
                float itemY = curY + i * InteractionMenuMetrics.ItemHeight;
                bool hovered = i == menu.HoveredIndex;

                if (hovered)
                {
                    sb.Draw(_bg, new Vector2(x + 2f, itemY + 1f), null,
                        new Vector2(w - 4f, InteractionMenuMetrics.ItemHeight - 2f), 0, Vector2.Zero,
                        new Color(0.22f, 0.38f, 0.58f, 0.95f));
                }

                var txtCol = hovered
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(0.85f, 0.85f, 0.9f, 1f);

                _font.DrawText(sb, menu.Items[i].Label,
                    new Vector2(x + InteractionMenuMetrics.Padding + InteractionMenuMetrics.TextPadX,
                                itemY + 3f),
                    InteractionMenuMetrics.TextScale, txtCol);
            }
        }

        /// <summary>Возвращает индекс пункта под курсором или -1.</summary>
        public static int HitTest(InteractionMenu menu,
            float mouseX, float mouseY,
            float screenWidth, float screenHeight)
        {
            if (!menu.IsOpen) return -1;

            int rows = menu.Items.Count + (string.IsNullOrEmpty(menu.Title) ? 0 : 1);

            ComputePanelRect(rows, screenWidth, screenHeight,
                out float x, out float y, out float w, out float h);

            if (mouseX < x || mouseX > x + w || mouseY < y || mouseY > y + h)
                return -1;

            float startY = y + InteractionMenuMetrics.Padding;
            if (!string.IsNullOrEmpty(menu.Title))
                startY += InteractionMenuMetrics.ItemHeight;

            float localY = mouseY - startY;
            if (localY < 0) return -1;

            int index = (int)(localY / InteractionMenuMetrics.ItemHeight);
            if (index < 0 || index >= menu.Items.Count) return -1;
            return index;
        }
    }
}