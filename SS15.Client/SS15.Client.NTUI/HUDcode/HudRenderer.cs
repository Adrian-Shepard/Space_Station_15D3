using System;
using System.Collections.Generic;
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

        private StatusPanel _statusPanel;
        private RuntimePanel _runtimePanel;
        private HandsHud _handsHud;
        private PocketHud _pocketHud;
        private CompassHud _compassHud;
        private NotificationLog _log;
        private ContextHints _hints;
        private DebugPanel _debugPanel;
        private ErrorIndicator _errorIndicator;
        private ErrorPanel _errorPanel;
        private InteractionMenuRenderer _interactionMenuRenderer;

        private Texture2D _slotTexture;
        private Texture2D _whiteTexture;
        private Texture2D _itemTexture;
        private Texture2D _beltTex;
        private Texture2D _backTex;

        private const int TileSize = 32;

        public bool DebugVisible { get; set; } = false;
        public bool ErrorPanelVisible { get; set; } = false;

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

            _statusPanel = new StatusPanel(_slotTexture, _whiteTexture, fontRenderer);
            _runtimePanel = new RuntimePanel(fontRenderer, whiteTexture);
            _handsHud = new HandsHud(armsPreviewTexture, itemTexture, whiteTexture);
            _pocketHud = new PocketHud(pocketTexture, itemTexture);
            _compassHud = new CompassHud(whiteTexture, whiteTexture, fontRenderer);
            _log = new NotificationLog(fontRenderer, whiteTexture);
            _hints = new ContextHints(fontRenderer, whiteTexture);
            _debugPanel = new DebugPanel(fontRenderer, whiteTexture);
            _errorIndicator = new ErrorIndicator(fontRenderer, whiteTexture);
            _errorPanel = new ErrorPanel(fontRenderer, whiteTexture);
            _interactionMenuRenderer = new InteractionMenuRenderer(fontRenderer, whiteTexture);
        }

        public void Notify(string text, Color? color = null) => _log.Add(text, color);

        public void Update(float deltaTime) => _log.Update(deltaTime);

        private List<string> BuildHints(bool hit, TileType tile)
        {
            var hints = new List<string>(3);
            if (!hit) return hints;

            switch (tile)
            {
                case TileType.Door:
                    hints.Add("ЛКМ — Открыть / Закрыть");
                    hints.Add("G — Удалить");
                    break;

                case TileType.Floor:
                    hints.Add("T — Стена   B — Дверь   Z — Провод");
                    hints.Add("V — Генератор   C — Лампа");
                    hints.Add("G — Убрать в Space");
                    break;

                case TileType.Space:
                    hints.Add("G — Восстановить пол");
                    break;

                case TileType.Wire:
                case TileType.Generator:
                case TileType.Lamp:
                    hints.Add("G — Удалить");
                    break;
            }
            return hints;
        }

        public void Draw(
            float fps,
            float playerUpdateMs, float atmosUpdateMs, float powerUpdateMs, float healthUpdateMs, float renderMs,
            int activeSlotIndex, int activeHandIndex,
            float screenWidth, float screenHeight,
            string hoverName, bool hoverHit, TileType hoverTile,
            int hoverTileX, int hoverTileY,
            GameRuntime runtime, RuntimeErrorLog errors,
            InteractionMenu interactionMenu)
        {
            var projection = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, -1, 1);
            var view = Matrix4x4.Identity;

            _spriteBatch.Begin(projection, view);

            // ==== Компас ====
            float bearingRad = MathF.Atan2(MathF.Cos(_player.Direction), -MathF.Sin(_player.Direction));
            float bearingDeg = (bearingRad * 180f / MathF.PI + 360f) % 360f;
            _compassHud.Draw(_spriteBatch, bearingDeg, screenWidth);

            // ==== Статус ====
            _statusPanel.Draw(_spriteBatch, _player);

            // ==== Runtime ====
            _runtimePanel.Draw(_spriteBatch, runtime, errors);

            // ==== Лог событий ====
            _log.Draw(_spriteBatch, screenWidth);

            // ==== Прицел ====
            DrawCrosshair(screenWidth, screenHeight);

            // ==== Имя тайла ====
            if (hoverHit && !string.IsNullOrEmpty(hoverName))
            {
                float textW = hoverName.Length * 32f * 0.7f * 0.5f;
                _fontRenderer.DrawText(_spriteBatch, hoverName,
                    new Vector2(screenWidth / 2f - textW / 2f, screenHeight / 2f + 30f),
                    0.5f, new Color(1f, 1f, 1f, 0.95f));
            }

            // ==== Подсказки (только когда меню закрыто) ====
            if (!interactionMenu.IsOpen)
            {
                var hints = BuildHints(hoverHit, hoverTile);
                if (hints.Count > 0)
                    _hints.Draw(_spriteBatch, hints, screenWidth, screenHeight);
            }

            // ==== Хотбар ====
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

            // ==== Индикатор ошибок ====
            _errorIndicator.Draw(_spriteBatch, errors, screenWidth, screenHeight);

            // ==== Debug (Tab) ====
            if (DebugVisible)
            {
                _debugPanel.Draw(
                    _spriteBatch, screenWidth,
                    fps, playerUpdateMs, atmosUpdateMs, powerUpdateMs, healthUpdateMs, renderMs,
                    _player, _atmosphereSystem,
                    hoverTileX, hoverTileY, hoverHit);
            }

            // ==== Панель ошибок (Esc) ====
            if (ErrorPanelVisible)
            {
                _errorPanel.Draw(_spriteBatch, errors, screenWidth, screenHeight);
            }

            // ==== Меню взаимодействий — фиксированное, поверх всего ====
            if (interactionMenu.IsOpen)
            {
                _interactionMenuRenderer.Draw(_spriteBatch, interactionMenu,
                    screenWidth, screenHeight);
            }

            _spriteBatch.End();
        }

        private void DrawCrosshair(float screenWidth, float screenHeight)
        {
            float cx = screenWidth / 2f;
            float cy = screenHeight / 2f;

            float armLen = 14f;
            float thick = 3f;
            float gap = 5f;

            var outline = new Color(0f, 0f, 0f, 0.9f);
            var fill = new Color(1f, 1f, 1f, 1f);

            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - thick / 2f - 1f, cy - armLen - gap - 1f), null,
                new Vector2(thick + 2f, armLen + 2f), 0, Vector2.Zero, outline);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - thick / 2f - 1f, cy + gap - 1f), null,
                new Vector2(thick + 2f, armLen + 2f), 0, Vector2.Zero, outline);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - armLen - gap - 1f, cy - thick / 2f - 1f), null,
                new Vector2(armLen + 2f, thick + 2f), 0, Vector2.Zero, outline);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx + gap - 1f, cy - thick / 2f - 1f), null,
                new Vector2(armLen + 2f, thick + 2f), 0, Vector2.Zero, outline);

            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - thick / 2f, cy - armLen - gap), null,
                new Vector2(thick, armLen), 0, Vector2.Zero, fill);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - thick / 2f, cy + gap), null,
                new Vector2(thick, armLen), 0, Vector2.Zero, fill);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - armLen - gap, cy - thick / 2f), null,
                new Vector2(armLen, thick), 0, Vector2.Zero, fill);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx + gap, cy - thick / 2f), null,
                new Vector2(armLen, thick), 0, Vector2.Zero, fill);

            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - 1.5f, cy - 1.5f), null,
                new Vector2(3f, 3f), 0, Vector2.Zero, outline);
            _spriteBatch.Draw(_whiteTexture,
                new Vector2(cx - 0.5f, cy - 0.5f), null,
                new Vector2(1f, 1f), 0, Vector2.Zero, fill);
        }

        private void DrawEquipmentSlot(Texture2D slotTex, int slotIndex, Vector2 position)
        {
            _spriteBatch.Draw(slotTex, position, null, Vector2.One, 0, Vector2.Zero, Color.White);
            var item = _player.Inventory.GetItem(slotIndex);
            if (item != null)
                DrawItem(item, position + new Vector2(16, 16));
        }

        private void DrawItem(Item item, Vector2 center)
        {
            float size = 16f;
            _spriteBatch.Draw(_itemTexture, center - new Vector2(size / 2f, size / 2f), null,
                new Vector2(size, size), 0, Vector2.Zero,
                new Color(item.Color[0] / 255f, item.Color[1] / 255f, item.Color[2] / 255f, 1f));
        }
    }
}