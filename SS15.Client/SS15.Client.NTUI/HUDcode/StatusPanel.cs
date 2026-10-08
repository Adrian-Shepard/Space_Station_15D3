using System.Numerics;
using Engine.Rendering;
using SS15.Client.Player;

namespace SS15.Client.NTUI
{
    /// Компактная панель статуса игрока: HP / Голод / Жажда.
    /// Слева сверху. Иконка + бар + число.
    public class StatusPanel
    {
        private readonly Texture2D _barBg;
        private readonly Texture2D _barFill;
        private readonly FontRenderer _font;

        private const float PanelX = 10f;
        private const float PanelY = 60f;
        private const float RowHeight = 22f;
        private const float LabelW = 44f;
        private const float BarW = 160f;
        private const float BarH = 14f;
        private const float NumberW = 44f;
        private const float LabelScale = 0.45f;

        // FontRenderer: 32px глифы, spacing 0.7
        private const float FontCellWidth = 32f;
        private const float FontSpacingFactor = 0.7f;

        public StatusPanel(Texture2D barBg, Texture2D barFill, FontRenderer font)
        {
            _barBg = barBg;
            _barFill = barFill;
            _font = font;
        }

        public void Draw(SpriteBatch sb, PlayerController player)
        {
            DrawRow(sb, 0, "HP",   player.Health / 100f, new Color(0.85f, 0.15f, 0.15f, 1f));
            DrawRow(sb, 1, "Food", 0.80f,                  new Color(0.90f, 0.65f, 0.20f, 1f));
            DrawRow(sb, 2, "H2O",  0.90f,                  new Color(0.25f, 0.60f, 0.90f, 1f));
        }

        private void DrawRow(SpriteBatch sb, int index, string label, float percent, Color fill)
        {
            percent = System.Math.Clamp(percent, 0f, 1f);

            float y = PanelY + index * RowHeight;
            float labelX = PanelX;
            float barX = PanelX + LabelW;
            float numX = barX + BarW + 6f;

            // Подпись (HP/Food/H2O)
            _font.DrawText(sb, label, new Vector2(labelX, y), LabelScale, new Color(1f, 1f, 1f, 1f));

            // Фон бара
            sb.Draw(_barBg, new Vector2(barX, y + 2f), null,
                new Vector2(BarW, BarH), 0, Vector2.Zero, new Color(0.08f, 0.08f, 0.10f, 0.75f));

            // Заливка
            float fillW = (BarW - 2f) * percent;
            if (fillW > 0.5f)
            {
                sb.Draw(_barFill, new Vector2(barX + 1f, y + 3f), null,
                    new Vector2(fillW, BarH - 2f), 0, Vector2.Zero, fill);
            }

            // Рамка
            DrawBorder(sb, new Vector2(barX, y + 2f), BarW, BarH, new Color(0f, 0f, 0f, 0.8f));

            // Число
            string num = ((int)(percent * 100)).ToString();
            _font.DrawText(sb, num, new Vector2(numX, y), LabelScale, new Color(1f, 1f, 1f, 1f));
        }

        private void DrawBorder(SpriteBatch sb, Vector2 pos, float w, float h, Color color)
        {
            // Верх
            sb.Draw(_barFill, new Vector2(pos.X, pos.Y), null,
                new Vector2(w, 1f), 0, Vector2.Zero, color);
            // Низ
            sb.Draw(_barFill, new Vector2(pos.X, pos.Y + h - 1f), null,
                new Vector2(w, 1f), 0, Vector2.Zero, color);
            // Лево
            sb.Draw(_barFill, new Vector2(pos.X, pos.Y), null,
                new Vector2(1f, h), 0, Vector2.Zero, color);
            // Право
            sb.Draw(_barFill, new Vector2(pos.X + w - 1f, pos.Y), null,
                new Vector2(1f, h), 0, Vector2.Zero, color);
        }
    }
}