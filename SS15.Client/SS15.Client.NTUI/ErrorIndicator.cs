using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.NTUI
{
    /// Маленькая плашка в правом нижнем углу: OK / Errors:N (F11).
    public class ErrorIndicator
    {
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        private const float Padding = 8f;
        private const float TextScale = 0.45f;
        private const float PanelH = 24f;

        public ErrorIndicator(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        public void Draw(SpriteBatch sb, RuntimeErrorLog log, float screenWidth, float screenHeight)
        {
            string text = log.HasErrors
                ? $"Errors: {log.TotalCount}  (F11)"
                : "OK";

            Color bgColor = log.HasErrors
                ? new Color(0.5f, 0.05f, 0.05f, 0.75f)
                : new Color(0.05f, 0.35f, 0.05f, 0.65f);

            Color textColor = log.HasErrors
                ? new Color(1f, 0.85f, 0.85f, 1f)
                : new Color(0.85f, 1f, 0.85f, 1f);

            float textW = text.Length * 32f * 0.7f * TextScale;
            float panelW = textW + Padding * 2f;

            float x = screenWidth - panelW - 10f;
            float y = screenHeight - PanelH - 10f;

            sb.Draw(_bg, new Vector2(x, y), null,
                new Vector2(panelW, PanelH), 0, Vector2.Zero, bgColor);

            _font.DrawText(sb, text,
                new Vector2(x + Padding, y + 4f),
                TextScale, textColor);
        }
    }
}