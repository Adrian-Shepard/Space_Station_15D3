using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.NTUI
{
    /// Контекстные подсказки — короткий список действий для тайла под прицелом.
    /// Показывается снизу по центру, чуть выше хотбара.
    public class ContextHints
    {
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        private const float TextScale = 0.45f;
        private const float Padding = 8f;
        private const float RowHeight = 20f;
        private const float FontCellWidth = 32f;
        private const float FontSpacingFactor = 0.7f;

        public ContextHints(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        public void Draw(SpriteBatch sb, List<string> hints, float screenWidth, float screenHeight)
        {
            if (hints == null || hints.Count == 0) return;

            float maxW = 0f;
            for (int i = 0; i < hints.Count; i++)
            {
                float w = hints[i].Length * FontCellWidth * FontSpacingFactor * TextScale;
                if (w > maxW) maxW = w;
            }

            float panelW = maxW + Padding * 2f;
            float panelH = hints.Count * RowHeight + Padding;
            float x = (screenWidth - panelW) / 2f;
            float y = screenHeight - 200f - panelH;   // 200 от низа = над хотбаром

            sb.Draw(_bg, new Vector2(x, y), null,
                new Vector2(panelW, panelH), 0, Vector2.Zero, new Color(0f, 0f, 0f, 0.5f));

            for (int i = 0; i < hints.Count; i++)
            {
                _font.DrawText(sb, hints[i],
                    new Vector2(x + Padding, y + Padding + i * RowHeight),
                    TextScale, new Color(1f, 1f, 0.85f, 1f));
            }
        }
    }
}