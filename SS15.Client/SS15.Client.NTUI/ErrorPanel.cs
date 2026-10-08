using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.NTUI
{
    public class ErrorPanel
    {
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        private const float Padding = 10f;
        private const float LineH = 17f;
        private const float TextScale = 0.42f;
        private const float FontCellWidth = 32f;
        private const float FontSpacingFactor = 0.7f;

        public ErrorPanel(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        public void Draw(SpriteBatch sb, RuntimeErrorLog log, float screenWidth, float screenHeight)
        {
            var lines = new List<(string text, Color color)>();

            lines.Add(("RUNTIME ERRORS", new Color(1f, 0.6f, 0.6f, 1f)));
            lines.Add(($"  Total: {log.TotalCount}  Warn: {log.WarningCount}  Err: {log.ErrorCount}  Crit: {log.CriticalCount}",
                new Color(0.9f, 0.9f, 0.9f, 1f)));

            if (log.CountsBySystem.Count > 0)
            {
                lines.Add(("", new Color(0f, 0f, 0f, 0f)));
                lines.Add(("By system:", new Color(0.75f, 0.85f, 0.95f, 1f)));
                foreach (var kv in log.CountsBySystem)
                    lines.Add(($"  {kv.Key}: {kv.Value}", new Color(0.8f, 0.8f, 0.8f, 1f)));
            }

            lines.Add(("", new Color(0f, 0f, 0f, 0f)));
            lines.Add(("Last errors (oldest → newest):", new Color(0.75f, 0.85f, 0.95f, 1f)));

            int start = System.Math.Max(0, log.Entries.Count - 20);
            for (int i = start; i < log.Entries.Count; i++)
            {
                var e = log.Entries[i];
                // [gameTime] System: Type Message
                string line = $"[{e.ShortGameTime}] {e.System}: {e.ExceptionType} {e.Message}";
                Color c = e.Severity switch
                {
                    RuntimeErrorSeverity.Critical => new Color(1f, 0.4f, 0.4f, 1f),
                    RuntimeErrorSeverity.Error    => new Color(1f, 0.7f, 0.7f, 1f),
                    _                              => new Color(1f, 1f, 0.7f, 1f),
                };
                lines.Add((line, c));
            }

            if (log.Entries.Count == 0)
                lines.Add(("  (no errors yet)", new Color(0.6f, 0.6f, 0.6f, 1f)));

            float maxW = 0f;
            for (int i = 0; i < lines.Count; i++)
            {
                float w = lines[i].text.Length * FontCellWidth * FontSpacingFactor * TextScale;
                if (w > maxW) maxW = w;
            }

            float panelW = maxW + Padding * 2f;
            float panelH = lines.Count * LineH + Padding * 3f;
            float x = 12f;
            float y = 190f;

            if (y + panelH > screenHeight - 60f)
                panelH = screenHeight - 60f - y;

            sb.Draw(_bg, new Vector2(x, y), null,
                new Vector2(panelW, panelH), 0, Vector2.Zero, new Color(0f, 0f, 0f, 0.75f));

            for (int i = 0; i < lines.Count; i++)
            {
                float ly = y + Padding + i * LineH;
                if (ly + LineH > y + panelH - LineH) break;
                _font.DrawText(sb, lines[i].text,
                    new Vector2(x + Padding, ly),
                    TextScale, lines[i].color);
            }

            _font.DrawText(sb, "F11 — закрыть. F10 — очистить.",
                new Vector2(x + Padding, y + panelH - LineH),
                TextScale, new Color(0.7f, 0.7f, 0.7f, 1f));
        }
    }
}