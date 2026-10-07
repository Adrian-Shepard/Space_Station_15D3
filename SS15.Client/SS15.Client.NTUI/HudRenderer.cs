using System;
using System.Numerics;
using Engine.Rendering;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Player;
using SS15.Client.Systems;

namespace SS15.Client.NTUI
{
    public class HudRenderer
    {
        private SpriteBatch _spriteBatch;
        private Camera2D _camera;
        private IMouse _mouse;
        private PlayerController _player;
        private AtmosphereSystem _atmosphereSystem;
        private PowerSystem _powerSystem;
        private FontRenderer _fontRenderer;

        private StatusBars _healthBar;
        private StatusBars _hungerBar;
        private StatusBars _thirstBar;
        private HandsHud _handsHud;
        private PocketHud _pocketHud;
        private CompassHud _compassHud;

        private Texture2D _slotTexture;
        private Texture2D _whiteTexture;
        private Texture2D _itemTexture;
        private Texture2D _beltTex;
        private Texture2D _backTex;

        private const int HudMargin = 10;
        private const int HealthBarWidth = 180;
        private const int HealthBarHeight = 18;
        private const int BarSpacing = 5;
        private const int TileSize = 32;

        public HudRenderer(
            SpriteBatch spriteBatch,
            Camera2D camera,
            IMouse mouse,
            PlayerController player,
            AtmosphereSystem atmosphereSystem,
            PowerSystem powerSystem,
            FontRenderer fontRenderer,
            Texture2D slotTexture,
            Texture2D whiteTexture,
            Texture2D itemTexture,
            Texture2D armsPreviewTexture,
            Texture2D pocketTexture,
            Texture2D beltTexture,
            Texture2D backTexture)
        {
            _spriteBatch = spriteBatch;
            _camera = camera;
            _mouse = mouse;
            _player = player;
            _atmosphereSystem = atmosphereSystem;
            _powerSystem = powerSystem;
            _fontRenderer = fontRenderer;
            _slotTexture = slotTexture;
            _whiteTexture = whiteTexture;
            _itemTexture = itemTexture;
            _beltTex = beltTexture;
            _backTex = backTexture;

            _healthBar = new StatusBars(slotTexture, whiteTexture);
            _hungerBar = new StatusBars(slotTexture, whiteTexture);
            _thirstBar = new StatusBars(slotTexture, whiteTexture);
            _handsHud = new HandsHud(armsPreviewTexture, itemTexture, whiteTexture);
            _pocketHud = new PocketHud(pocketTexture, itemTexture);
            _compassHud = new CompassHud(whiteTexture, whiteTexture, fontRenderer);
        }

        public void Draw(
            float fps,
            float playerUpdateMs, float atmosUpdateMs, float powerUpdateMs, float healthUpdateMs, float renderMs,
            int activeSlotIndex, int activeHandIndex,
            float screenWidth, float screenHeight)
        {
            var projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
            var view = Matrix4x4.Identity;

            _spriteBatch.Begin(projection, view);

            float barX = HudMargin;
            float barY = HudMargin;

            _healthBar.Draw(_spriteBatch, new Vector2(barX, barY), _player.Health / 100f, HealthBarWidth, HealthBarHeight, new Color(1f, 0f, 0f, 1f));
            _hungerBar.Draw(_spriteBatch, new Vector2(barX, barY + HealthBarHeight + BarSpacing), 0.8f, HealthBarWidth, HealthBarHeight, new Color(1f, 0.6f, 0f, 1f));
            _thirstBar.Draw(_spriteBatch, new Vector2(barX, barY + 2 * (HealthBarHeight + BarSpacing)), 0.9f, HealthBarWidth, HealthBarHeight, new Color(0f, 0.5f, 1f, 1f));

            float textY = barY + 3 * (HealthBarHeight + BarSpacing) + 15;
            _fontRenderer.DrawText(_spriteBatch, $"Health: {_player.Health}", new Vector2(barX, textY), 0.5f, new Color(0, 0, 0, 1));
            _fontRenderer.DrawText(_spriteBatch, $"Hunger: 80", new Vector2(barX, textY + 20), 0.5f, new Color(0, 0, 0, 1));
            _fontRenderer.DrawText(_spriteBatch, $"Thirst: 90", new Vector2(barX, textY + 40), 0.5f, new Color(0, 0, 0, 1));

            // Атмосфера под игроком
            int playerTileX = (int)(_player.Position.X / TileSize);
            int playerTileY = (int)(_player.Position.Y / TileSize);
            var playerAtmosNullable = _atmosphereSystem.GetAtmosphereAt(playerTileX, playerTileY);
            if (playerAtmosNullable.HasValue)
            {
                var playerAtmos = playerAtmosNullable.Value;
                _fontRenderer.DrawText(_spriteBatch, "Player Atmos:", new Vector2(barX, textY + 70), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  P: {playerAtmos.Pressure:F1} kPa", new Vector2(barX, textY + 90), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  T: {playerAtmos.Temperature - 273.15f:F1} C", new Vector2(barX, textY + 110), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  O2: {playerAtmos.OxygenPercent:F1}%", new Vector2(barX, textY + 130), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  N2: {playerAtmos.NitrogenPercent:F1}%", new Vector2(barX, textY + 150), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  CO2: {playerAtmos.CarbonDioxidePercent:F2}%", new Vector2(barX, textY + 170), 0.5f, new Color(0, 0, 0, 1));
            }

            // Атмосфера под курсором
            Vector2 mouseScreen = new Vector2(_mouse.Position.X, _mouse.Position.Y);
            Vector2 mouseWorld = ScreenToWorld(mouseScreen, screenWidth, screenHeight);
            int mouseTileX = (int)(mouseWorld.X / TileSize);
            int mouseTileY = (int)(mouseWorld.Y / TileSize);
            var mouseAtmosNullable = _atmosphereSystem.GetAtmosphereAt(mouseTileX, mouseTileY);
            if (mouseAtmosNullable.HasValue)
            {
                var mouseAtmos = mouseAtmosNullable.Value;
                _fontRenderer.DrawText(_spriteBatch, "Cursor Atmos:", new Vector2(barX, textY + 220), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  P: {mouseAtmos.Pressure:F1} kPa", new Vector2(barX, textY + 240), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  T: {mouseAtmos.Temperature - 273.15f:F1} C", new Vector2(barX, textY + 260), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  O2: {mouseAtmos.OxygenPercent:F1}%", new Vector2(barX, textY + 280), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  N2: {mouseAtmos.NitrogenPercent:F1}%", new Vector2(barX, textY + 300), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  CO2: {mouseAtmos.CarbonDioxidePercent:F2}%", new Vector2(barX, textY + 320), 0.5f, new Color(0, 0, 0, 1));
                _fontRenderer.DrawText(_spriteBatch, $"  Plasma: {mouseAtmos.PlasmaPercent:F1}%", new Vector2(barX, textY + 340), 0.5f, new Color(0, 0, 0, 1));
            }

            bool powered = _powerSystem.IsPowered(mouseTileX, mouseTileY);
            _fontRenderer.DrawText(_spriteBatch, $"Powered: {(powered ? "Yes" : "No")}", new Vector2(barX, textY + 320), 0.5f, powered ? new Color(0, 0.8f, 0, 1) : new Color(0, 0, 0, 1));

            _fontRenderer.DrawText(_spriteBatch, "U/J: O2  I/K: Temp  O/L: N2  N/M: CO2  Y/H: Plasma  T: Wall  B: Door  Z: Wire  V: Gen  C: Lamp  X: Switch Hand  E: Pocket  R: Use", new Vector2(barX, textY + 360), 0.4f, new Color(0, 0, 0, 1));

            // Производительность
            float fpsX = screenWidth - 250;
            float fpsY = HudMargin;
            _fontRenderer.DrawText(_spriteBatch, $"FPS: {fps:F0}", new Vector2(fpsX, fpsY), 0.5f, new Color(0, 0, 0, 1));
            DrawPerformanceGraph(fpsX, fpsY + 20, playerUpdateMs, atmosUpdateMs, powerUpdateMs, healthUpdateMs, renderMs);

            // ===== Компас =====
            float bearingRad = MathF.Atan2(MathF.Cos(_player.Direction), -MathF.Sin(_player.Direction));
            float bearingDeg = (bearingRad * 180f / MathF.PI + 360f) % 360f;
            _compassHud.Draw(_spriteBatch, bearingDeg, screenWidth);

            // Нижняя часть HUD
            float panelX = (screenWidth - 66) / 2f;
            float panelY = screenHeight - 45 - 10;
            _handsHud.Draw(_spriteBatch, _player.Inventory, activeHandIndex, new Vector2(panelX, panelY));

            float backX = panelX - 32 - 5;
            float backY = panelY + (45 - 32) / 2f;
            DrawEquipmentSlot(_backTex, 7, new Vector2(backX, backY));

            float beltX = backX - 32 - 5;
            float beltY = backY;
            DrawEquipmentSlot(_beltTex, 6, new Vector2(beltX, beltY));

            float pocketsX = panelX + 66 + 5;
            float pocketsY = backY;
            _pocketHud.Draw(_spriteBatch, _player.Inventory, new Vector2(pocketsX, pocketsY));

            _spriteBatch.End();
        }

        private Vector2 ScreenToWorld(Vector2 screenPos, float screenWidth, float screenHeight)
        {
            float scaleX = screenWidth / 800f;
            float scaleY = screenHeight / 600f;
            return new Vector2(
                screenPos.X / scaleX / _camera.Zoom + _camera.Position.X,
                screenPos.Y / scaleY / _camera.Zoom + _camera.Position.Y
            );
        }

        private void DrawEquipmentSlot(Texture2D slotTex, int slotIndex, Vector2 position)
        {
            _spriteBatch.Draw(slotTex, position, null, Vector2.One, 0, Vector2.Zero, Color.White);
            var item = _player.Inventory.GetItem(slotIndex);
            if (item != null)
            {
                DrawItem(item, position + new Vector2(16, 16));
            }
        }

        private void DrawItem(Item item, Vector2 center)
        {
            float size = 16f;
            _spriteBatch.Draw(_itemTexture, center - new Vector2(size / 2f, size / 2f), null,
                new Vector2(size, size), 0, Vector2.Zero,
                new Color(item.Color[0] / 255f, item.Color[1] / 255f, item.Color[2] / 255f, 1f));
        }

        private void DrawPerformanceGraph(float x, float y, float playerMs, float atmosMs, float powerMs, float healthMs, float renderMs)
        {
            float maxMs = 5f;
            float barWidth = 150f;
            float barHeight = 8f;
            float gap = 2f;

            string[] labels = { "Player", "Atmos", "Power", "Health", "Render" };
            float[] times = { playerMs, atmosMs, powerMs, healthMs, renderMs };
            Color[] colors = {
                new Color(0, 0.5f, 1, 1),
                new Color(0, 0.8f, 0, 1),
                new Color(1, 0.6f, 0, 1),
                new Color(1, 0, 0, 1),
                new Color(0.8f, 0.2f, 0.8f, 1)
            };

            for (int i = 0; i < labels.Length; i++)
            {
                _spriteBatch.Draw(_slotTexture, new Vector2(x, y + i * (barHeight + gap)), null,
                    new Vector2(barWidth, barHeight), 0, Vector2.Zero, new Color(0.2f, 0.2f, 0.2f, 1));

                float fill = Math.Clamp(times[i] / maxMs, 0, 1);
                _spriteBatch.Draw(_whiteTexture, new Vector2(x, y + i * (barHeight + gap)), null,
                    new Vector2(barWidth * fill, barHeight), 0, Vector2.Zero, colors[i]);

                _fontRenderer.DrawText(_spriteBatch, $"{labels[i]}: {times[i]:F2}ms", new Vector2(x + barWidth + 5, y + i * (barHeight + gap)), 0.4f, new Color(0, 0, 0, 1));
            }
        }
    }
}