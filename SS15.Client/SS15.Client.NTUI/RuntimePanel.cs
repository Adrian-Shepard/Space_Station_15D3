using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.NTUI
{
    /// Левый верх: uptime, game time, frames, fps, errors count.
    /// Полупрозрачная, компактная.
    public class RuntimePanel
    {
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        private const float PanelX = 10f;
        private const float PanelY = 132f;
        private const float PanelW = 210f;
        private const float RowH = 18f;
        private const float Padding = 6f;
        private const float TextScale = 0.42f;

        public RuntimePanel(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        public void Draw(SpriteBatch sb, GameRuntime runtime, RuntimeErrorLog errors)
        {
            // Строки панели
            string[] lines =
            {
                $"Uptime:  {runtime.FormatRealTime()}",
                $"Game:    {runtime.FormatGameTime()}",
                $"Frames:  {runtime.FrameCount}",
                $"FPS:     {runtime.CurrentFps:F0}",
            };

            bool hasErr = errors.HasErrors;
            string errLine = hasErr
                ? $"Errors:  {errors.TotalCount}  (F11)"
                : "Errors:  OK";

            int lineCount = lines.Length + 1;
            float panelH = lineCount * RowH + Padding * 2f;

            // Фон
            sb.Draw(_bg, new Vector2(PanelX, PanelY), null,
                new Vector2(PanelW, panelH), 0, Vector2.Zero,
                new Color(0f, 0f, 0f, 0.45f));

            // Текст
            var timeColor = new Color(0.75f, 0.85f, 0.95f, 1f);
            for (int i = 0; i < lines.Length; i++)
            {
                _font.DrawText(sb, lines[i],
                    new Vector2(PanelX + Padding, PanelY + Padding + i * RowH),
                    TextScale, timeColor);
            }

            var errColor = hasErr
                ? new Color(1f, 0.5f, 0.5f, 1f)
                : new Color(0.6f, 1f, 0.6f, 1f);

            _font.DrawText(sb, errLine,
                new Vector2(PanelX + Padding, PanelY + Padding + lines.Length * RowH),
                TextScale, errColor);
        }
    }
}