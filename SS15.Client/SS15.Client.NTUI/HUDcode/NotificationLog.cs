using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.NTUI
{
    /// Список временных сообщений в правом верхнем углу.
    /// Каждое живёт Lifetime секунд, потом исчезает.
    public class NotificationLog
    {
        private class Entry
        {
            public string Text;
            public float Remaining;
            public Color Color;
        }

        private readonly List<Entry> _entries = new(16);
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        private const float Lifetime = 5f;
        private const int MaxEntries = 6;
        private const float TextScale = 0.45f;
        private const float RowHeight = 20f;
        private const float Padding = 8f;
        private const float FontCellWidth = 32f;
        private const float FontSpacingFactor = 0.7f;

        public NotificationLog(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        public void Add(string text, Color? color = null)
        {
            _entries.Add(new Entry
            {
                Text = text,
                Remaining = Lifetime,
                Color = color ?? new Color(1f, 1f, 1f, 1f)
            });

            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0);
        }

        public void Update(float deltaTime)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                _entries[i].Remaining -= deltaTime;
                if (_entries[i].Remaining <= 0f)
                    _entries.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch sb, float screenWidth)
        {
            if (_entries.Count == 0) return;

            float maxTextW = 0f;
            for (int i = 0; i < _entries.Count; i++)
            {
                float w = _entries[i].Text.Length * FontCellWidth * FontSpacingFactor * TextScale;
                if (w > maxTextW) maxTextW = w;
            }

            float panelW = maxTextW + Padding * 2f;
            float panelH = _entries.Count * RowHeight + Padding;
            float x = screenWidth - panelW - 12f;
            float y = 60f;

            // Фон
            sb.Draw(_bg, new Vector2(x, y), null,
                new Vector2(panelW, panelH), 0, Vector2.Zero, new Color(0f, 0f, 0f, 0.45f));

            // Записи: старые сверху, новые снизу
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                float alpha = Math.Min(1f, e.Remaining / 1.0f);   // плавное исчезновение
                var color = new Color(e.Color.R, e.Color.G, e.Color.B, e.Color.A * alpha);

                _font.DrawText(sb, e.Text,
                    new Vector2(x + Padding, y + Padding + i * RowHeight),
                    TextScale, color);
            }
        }
    }
}