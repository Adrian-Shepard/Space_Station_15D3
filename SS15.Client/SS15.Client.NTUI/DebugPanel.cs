using System.Numerics;
using Engine.Rendering;
using SS15.Client.Player;
using SS15.Client.Systems;

namespace SS15.Client.NTUI
{
    /// Отладочная панель: FPS, тайминги систем, атмосфера под игроком и прицелом.
    /// Скрыта по умолчанию. Включается клавишей F12.
    public class DebugPanel
    {
        private readonly FontRenderer _font;
        private readonly Texture2D _bg;

        private const float TextScale = 0.42f;
        private const float Padding = 8f;
        private const float LineH = 18f;
        private const float FontCellWidth = 32f;
        private const float FontSpacingFactor = 0.7f;

        public DebugPanel(FontRenderer font, Texture2D background)
        {
            _font = font;
            _bg = background;
        }

        public void Draw(
            SpriteBatch sb,
            float screenWidth,
            float fps,
            float playerMs, float atmosMs, float powerMs, float healthMs, float renderMs,
            PlayerController player,
            AtmosphereSystem atmosphere,
            int hoverTileX, int hoverTileY,
            bool hoverHit)
        {
            // Формируем строки
            var lines = new System.Collections.Generic.List<string>
            {
                $"FPS: {fps:F0}",
                $"Player: {playerMs:F2}ms  Atmos: {atmosMs:F2}ms",
                $"Power:  {powerMs:F2}ms  Health: {healthMs:F2}ms",
                $"Render: {renderMs:F2}ms",
                $"Pos: ({player.Position.X:F0}, {player.Position.Y:F0})"
            };

            // Атмосфера под игроком
            int ptx = (int)(player.Position.X / 32);
            int pty = (int)(player.Position.Y / 32);
            var pa = atmosphere.GetAtmosphereAt(ptx, pty);
            if (pa.HasValue)
            {
                var a = pa.Value;
                lines.Add($"Player tile: ({ptx},{pty})");
                lines.Add($"  P={a.Pressure:F1}kPa T={a.Temperature - 273.15f:F0}C");
                lines.Add($"  O2={a.OxygenPercent:F0}% N2={a.NitrogenPercent:F0}% CO2={a.CarbonDioxidePercent:F1}%");
            }
            else
            {
                lines.Add($"Player tile: ({ptx},{pty}) — no atmos");
            }

            // Атмосфера под прицелом
            if (hoverHit)
            {
                var ha = atmosphere.GetAtmosphereAt(hoverTileX, hoverTileY);
                lines.Add($"Hover tile: ({hoverTileX},{hoverTileY})");
                if (ha.HasValue)
                {
                    var a = ha.Value;
                    lines.Add($"  P={a.Pressure:F1}kPa T={a.Temperature - 273.15f:F0}C");
                    lines.Add($"  O2={a.OxygenPercent:F0}% N2={a.NitrogenPercent:F0}%");
                    lines.Add($"  CO2={a.CarbonDioxidePercent:F1}% Plasma={a.PlasmaPercent:F1}%");
                }
                else
                {
                    lines.Add("  no atmos (wall/space)");
                }
            }

            // Размер панели
            float maxW = 0f;
            for (int i = 0; i < lines.Count; i++)
            {
                float w = lines[i].Length * FontCellWidth * FontSpacingFactor * TextScale;
                if (w > maxW) maxW = w;
            }

            float panelW = maxW + Padding * 2f;
            float panelH = lines.Count * LineH + Padding * 2f;
            float x = screenWidth - panelW - 12f;
            float y = 10f;

            // Фон
            sb.Draw(_bg, new Vector2(x, y), null,
                new Vector2(panelW, panelH), 0, Vector2.Zero, new Color(0f, 0f, 0f, 0.6f));

            // Текст
            var color = new Color(0.85f, 1f, 0.85f, 1f);
            for (int i = 0; i < lines.Count; i++)
            {
                _font.DrawText(sb, lines[i],
                    new Vector2(x + Padding, y + Padding + i * LineH),
                    TextScale, color);
            }
        }
    }
}