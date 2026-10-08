using System;
using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.NTUI
{
    /// Ленточный компас вверху экрана.
    /// Стрелка фиксирована по центру, метки «плывут» при повороте.
    public class CompassHud
    {
        private readonly Texture2D _bgTex;
        private readonly Texture2D _tickTex;
        private readonly FontRenderer _font;

        // Ширина ленты — clamp по ширине экрана
        private const float MaxBarWidth = 520f;
        private const float MinBarWidth = 240f;
        private const float BarHeight = 34f;
        private const float FovDegrees = 180f;

        // FontRenderer: каждая глифа 32px, spacing 0.7
        private const float FontCellWidth = 32f;
        private const float FontSpacingFactor = 0.7f;

        public CompassHud(Texture2D backgroundTexture, Texture2D tickTexture, FontRenderer font)
        {
            _bgTex = backgroundTexture;
            _tickTex = tickTexture;
            _font = font;
        }

        public void Draw(SpriteBatch sb, float bearingDeg, float screenWidth)
        {
            float barWidth = Math.Clamp(screenWidth * 0.6f, MinBarWidth, MaxBarWidth);
            float pixelsPerDegree = barWidth / FovDegrees;

            float centerX = screenWidth / 2f;
            float y = 8f;
            float barLeft = centerX - barWidth / 2f;
            float halfWidth = barWidth / 2f;

            // Фон ленты
            sb.Draw(_bgTex, new Vector2(barLeft, y), null,
                new Vector2(barWidth, BarHeight), 0, Vector2.Zero,
                new Color(0f, 0f, 0f, 0.6f));

            // Штрихи каждые 15°
            for (int deg = 0; deg < 360; deg += 15)
            {
                float delta = NormalizeDelta(deg - bearingDeg);
                float x = centerX + delta * pixelsPerDegree;
                if (MathF.Abs(delta) > FovDegrees / 2f) continue;
                if (x < barLeft + 1f || x > barLeft + barWidth - 1f) continue;

                bool isMajor = deg % 90 == 0;
                bool isMid = deg % 45 == 0;

                float tickH = isMajor ? 12f : (isMid ? 8f : 4f);
                float tickW = isMajor ? 2f : 1f;
                var color = isMajor
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(0.75f, 0.75f, 0.75f, 0.85f);

                sb.Draw(_tickTex,
                    new Vector2(x - tickW / 2f, y + BarHeight - tickH),
                    null, new Vector2(tickW, tickH), 0, Vector2.Zero, color);
            }

            // Кардинальные буквы
            DrawLabel(sb, "N", 0f,   bearingDeg, centerX, y + 3f, pixelsPerDegree, halfWidth, new Color(1f, 0.35f, 0.35f, 1f), 0.55f);
            DrawLabel(sb, "E", 90f,  bearingDeg, centerX, y + 3f, pixelsPerDegree, halfWidth, new Color(1f, 1f, 1f, 1f),        0.55f);
            DrawLabel(sb, "S", 180f, bearingDeg, centerX, y + 3f, pixelsPerDegree, halfWidth, new Color(1f, 1f, 1f, 1f),        0.55f);
            DrawLabel(sb, "W", 270f, bearingDeg, centerX, y + 3f, pixelsPerDegree, halfWidth, new Color(1f, 1f, 1f, 1f),        0.55f);

            // Промежуточные буквы
            DrawLabel(sb, "NE", 45f,  bearingDeg, centerX, y + 5f, pixelsPerDegree, halfWidth, new Color(0.7f, 0.7f, 0.7f, 1f), 0.4f);
            DrawLabel(sb, "SE", 135f, bearingDeg, centerX, y + 5f, pixelsPerDegree, halfWidth, new Color(0.7f, 0.7f, 0.7f, 1f), 0.4f);
            DrawLabel(sb, "SW", 225f, bearingDeg, centerX, y + 5f, pixelsPerDegree, halfWidth, new Color(0.7f, 0.7f, 0.7f, 1f), 0.4f);
            DrawLabel(sb, "NW", 315f, bearingDeg, centerX, y + 5f, pixelsPerDegree, halfWidth, new Color(0.7f, 0.7f, 0.7f, 1f), 0.4f);

            // Центральный маркер (жёлтая вертикальная полоска)
            float markerW = 2f;
            sb.Draw(_tickTex,
                new Vector2(centerX - markerW / 2f, y - 4f),
                null, new Vector2(markerW, BarHeight + 8f), 0, Vector2.Zero,
                new Color(1f, 0.85f, 0.1f, 1f));

            // Числовой курс под лентой
            string degText = ((int)MathF.Round(bearingDeg) % 360).ToString("000");
            float textW = TextWidth(degText, 0.5f);
            _font.DrawText(sb, degText,
                new Vector2(centerX - textW / 2f, y + BarHeight + 2f),
                0.5f, new Color(1f, 0.85f, 0.1f, 1f));
        }

        private void DrawLabel(SpriteBatch sb, string label, float deg, float bearing,
            float centerX, float y, float pixelsPerDegree, float halfWidth,
            Color color, float scale)
        {
            float delta = NormalizeDelta(deg - bearing);
            if (MathF.Abs(delta) > FovDegrees / 2f - 6f) return;

            float x = centerX + delta * pixelsPerDegree;
            if (x < centerX - halfWidth + 6f || x > centerX + halfWidth - 6f) return;

            float w = TextWidth(label, scale);
            _font.DrawText(sb, label, new Vector2(x - w / 2f, y), scale, color);
        }

        private static float TextWidth(string s, float scale)
            => s.Length * FontCellWidth * FontSpacingFactor * scale;

        private static float NormalizeDelta(float d)
        {
            while (d > 180f) d -= 360f;
            while (d < -180f) d += 360f;
            return d;
        }
    }
}