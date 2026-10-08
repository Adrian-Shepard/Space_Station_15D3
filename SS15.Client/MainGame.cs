#nullable disable
using System;
using System.Numerics;
using Engine.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SS15.Client.Core;
using SS15.Client.Input;
using SS15.Client.NTUI;
using SS15.Client.Render;
using SS15.Client.Render.GeneratedSprites;
using SS15.Client.Systems;
using SS15.Client.Objects;
using Framebuffer = SS15.Client.Render.Framebuffer;

namespace SS15.Client
{
    public class MainGame : GraphicsDevice
    {
        private SpriteBatch _spriteBatch;
        private IInputContext _inputContext;
        private IKeyboard _keyboard;
        private IMouse _mouse;

        private GameState _gameState;
        private Camera3D _camera3D;
        private WorldRenderer3D _world3D;
        private HudRenderer _hudRenderer;
        private InputHandler _inputHandler;
        private Framebuffer _framebuffer;

        private RaycastHit _currentHit;

        // Меню взаимодействий (Alt + ПКМ)
        private readonly InteractionMenu _interactionMenu = new();
        private bool _lmbInMenuPrev;
        private bool _wasAltDown;

        private Texture2D _floorTex, _spaceTex;
        private Texture2D _wallSingleTex, _wallNSTex, _wallWETex;
        private Texture2D _wireTex;
        private Texture2D _gridTex, _playerTex, _itemTex, _whiteTexture, _slotTexture;
        private Texture2D _generatorTex, _lampOnTex, _lampOffTex;
        private Texture2D _doorOpenTex, _doorClosedTex;
        private Texture2D _armsPreviewTex, _pocketTex, _beltTex, _backTex;
        private Texture2D _highlightTex;

        private System.Diagnostics.Stopwatch _fpsTimer;
        private int _frameCount;
        private float _fps;
        private float _playerUpdateMs, _atmosUpdateMs, _powerUpdateMs, _healthUpdateMs, _renderMs;

        private int _activeSlotIndex = 0;
        private int _activeHandIndex = 0;

        private float _actualWidth, _actualHeight;
        private Vector2 _windowScale;

        private DateTime _lastCamLog = DateTime.MinValue;

        private bool _savePrev;
        private bool _loadPrev;
        private bool _debugPrev;
        private bool _errPanelPrev;
        private bool _clearErrPrev;

        private const int DoorWidth = 2;
        private const float ReachDistance = 5.0f;

        public MainGame() : base(800, 600, "Space Station 15 — 3D")
        {
        }

        protected override void OnLoad()
        {
            Logger.Info("MainGame", "OnLoad started");

            LoadAllTextures();

            _spriteBatch = new SpriteBatch(Gl);
            Logger.Info("MainGame", "SpriteBatch created");

            LogOpenGLInfo();

            _actualWidth = Window.Size.X;
            _actualHeight = Window.Size.Y;
            Gl.Viewport(0, 0, (uint)_actualWidth, (uint)_actualHeight);
            UpdateWindowScale();

            Logger.Info("MainGame", $"Window size: {_actualWidth}x{_actualHeight}");

            _framebuffer = new Framebuffer(Gl, (int)_actualWidth, (int)_actualHeight);

            _fpsTimer = System.Diagnostics.Stopwatch.StartNew();

            _inputContext = Window.CreateInput();
            _keyboard = _inputContext.Keyboards[0];
            _mouse = _inputContext.Mice[0];
            Logger.Info("MainGame", "Input initialized");

            _gameState = new GameState();
            _gameState.Initialize(_keyboard);
            Logger.Info("MainGame", $"GameState initialized. Map: {_gameState.Map.Width}x{_gameState.Map.Height}");
            Logger.Info("MainGame", $"Player spawn: {_gameState.Player.Position}");

            int spawnTx = (int)(_gameState.Player.Position.X / _gameState.Map.TileSize);
            int spawnTy = (int)(_gameState.Player.Position.Y / _gameState.Map.TileSize);
            var spawnTile = _gameState.Map.GetTile(spawnTx, spawnTy);
            Logger.Info("MainGame", $"Tile under player ({spawnTx},{spawnTy}): {spawnTile}");

            _camera3D = new Camera3D
            {
                ViewportWidth = (int)_actualWidth,
                ViewportHeight = (int)_actualHeight
            };

            _world3D = new WorldRenderer3D(
                Gl,
                _gameState.Map,
                _gameState.Power,
                _gameState.Atmosphere);

            _world3D.SetTextures(
                _wallSingleTex, _wallNSTex, _wallWETex,
                _floorTex, _spaceTex,
                _wireTex,
                _doorOpenTex, _doorClosedTex,
                _generatorTex, _lampOnTex, _lampOffTex,
                _highlightTex);

            Logger.Info("MainGame", "World3D initialized");

            var hudCamera = new Camera2D
            {
                ViewportWidth = 800,
                ViewportHeight = 600,
                Zoom = 1f,
                IsoYFactor = 1f,
                TileHeight = 0f
            };

            _hudRenderer = new HudRenderer(
                _spriteBatch, hudCamera, _mouse,
                _gameState.Player, _gameState.Atmosphere, _gameState.Power,
                new FontRenderer(Gl), _slotTexture, _whiteTexture, _itemTex,
                _armsPreviewTex, _pocketTex, _beltTex, _backTex);

            Logger.Info("MainGame", "HUD initialized");

            _gameState.Errors.OnNewError += OnRuntimeError;

            _inputHandler = new InputHandler(_keyboard, _mouse, _gameState.Map, _gameState.Player);
            SubscribeInputEvents();

            _gameState.Player.Direction = 0f;
            _gameState.Player.Pitch = 0f;

            _hudRenderer.Notify("Добро пожаловать на борт, инженер.", new Color(1f, 0.9f, 0.6f, 1f));
            _hudRenderer.Notify("P — сохранить, Home — загрузить", new Color(0.8f, 0.8f, 1f, 1f));
            _hudRenderer.Notify("Alt — отпустить курсор, Alt+ПКМ — меню", new Color(0.8f, 1f, 0.8f, 1f));

            Logger.Info("MainGame", "OnLoad complete");
        }

        private void OnRuntimeError(RuntimeErrorEntry entry)
        {
            var color = entry.Severity == RuntimeErrorSeverity.Critical
                ? new Color(1f, 0.4f, 0.4f, 1f)
                : new Color(1f, 0.85f, 0.5f, 1f);
            _hudRenderer.Notify($"Ошибка [{entry.System}]: {entry.Message}", color);
        }

        private void LoadAllTextures()
        {
            try
            {
                _floorTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Floors/floor.png", (gl) => SpriteGenerator.CreateFloorTexture(gl, 32));
                _spaceTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Floors/space.png", (gl) => SpriteGenerator.CreateSpaceTexture(gl, 32));
                _wallSingleTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Walls/Wall_METAL/Wall_METAL_SINGLE.png", (gl) => SpriteGenerator.CreateWallTexture(gl, 32));
                _wallNSTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Walls/Wall_METAL/Directions/Wall_METAL_North_South.png", (gl) => SpriteGenerator.CreateWallTexture(gl, 32));
                _wallWETex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Walls/Wall_METAL/Directions/Wall_METAL_West_East.png", (gl) => SpriteGenerator.CreateWallTexture(gl, 32));
                _playerTex = SpriteGenerator.LoadTexture(Gl, "Sprites/player.png", (gl) => SpriteGenerator.CreatePlayerTexture(gl, 32));

                _wireTex = WireSpriteGenerator.LoadOrCreate(Gl);
                _generatorTex = GeneratorSpriteGenerator.LoadOrCreate(Gl);
                _lampOffTex = LampSpriteGenerator.LoadOrCreate(Gl);
                _lampOnTex = LampSpriteGenerator.LoadOrCreateOn(Gl);
                _doorOpenTex = DoorSpriteGenerator.LoadOrCreateOpen(Gl);
                _doorClosedTex = DoorSpriteGenerator.LoadOrCreateClosed(Gl);

                _armsPreviewTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Misc/UI/Arms_Preview.png");
                _pocketTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Misc/UI/UI_Pocket.png");
                _beltTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Misc/UI/UI_Belt.png");
                _backTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Misc/UI/UI_Back.png");

                _gridTex = SpriteGenerator.CreateSolidTexture(Gl, new byte[] { 0, 0, 0, 255 });
                _itemTex = SpriteGenerator.CreateSolidTexture(Gl, new byte[] { 255, 255, 255, 255 });
                _whiteTexture = SpriteGenerator.CreateSolidTexture(Gl, new byte[] { 255, 255, 255, 255 });
                _slotTexture = SpriteGenerator.CreateSolidTexture(Gl, new byte[] { 50, 50, 50, 255 });
                _highlightTex = SpriteGenerator.CreateSolidTexture(Gl, new byte[] { 255, 210, 0, 130 });

                Logger.Info("MainGame", "All textures loaded");
            }
            catch (Exception ex)
            {
                Logger.Error("MainGame", "Texture loading failed", ex);
                throw;
            }
        }

        private void LogOpenGLInfo()
        {
            try
            {
                string vendor = Gl.GetStringS(StringName.Vendor);
                string renderer = Gl.GetStringS(StringName.Renderer);
                string version = Gl.GetStringS(StringName.Version);
                string glsl = Gl.GetStringS(StringName.ShadingLanguageVersion);

                Logger.Info("GL", $"Vendor:   {vendor}");
                Logger.Info("GL", $"Renderer: {renderer}");
                Logger.Info("GL", $"Version:  {version}");
                Logger.Info("GL", $"GLSL:     {glsl}");

                int depthBits = Gl.GetInteger((GetPName)0x0D56);
                Logger.Info("GL", $"Default framebuffer depth bits: {depthBits} (0 = need custom FBO)");
            }
            catch (Exception ex)
            {
                Logger.Error("GL", "Failed to get OpenGL info", ex);
            }
        }

        private void UpdateWindowScale()
        {
            _windowScale = new Vector2(_actualWidth / 800f, _actualHeight / 600f);
        }

        protected override void OnResize(Vector2D<int> size)
        {
            base.OnResize(size);
            _actualWidth = size.X;
            _actualHeight = size.Y;
            Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            UpdateWindowScale();

            if (_camera3D != null)
            {
                _camera3D.ViewportWidth = size.X;
                _camera3D.ViewportHeight = size.Y;
            }

            if (_framebuffer != null && size.X > 0 && size.Y > 0)
                _framebuffer.Resize(size.X, size.Y);
        }

        private void SubscribeInputEvents()
        {
            _inputHandler.LeftClick           += (x, y) => SafeCall("LeftClick", () => OnLeftClick(x, y));
            _inputHandler.PlaceWall           += (x, y) => SafeCall("PlaceWall", () => OnPlaceWall(x, y));
            _inputHandler.PlaceDoor           += (x, y) => SafeCall("PlaceDoor", () => OnPlaceDoor(x, y));
            _inputHandler.PlaceWire           += (x, y) => SafeCall("PlaceWire", () => OnPlaceWire(x, y));
            _inputHandler.PlaceGenerator      += (x, y) => SafeCall("PlaceGenerator", () => OnPlaceGenerator(x, y));
            _inputHandler.PlaceLamp           += (x, y) => SafeCall("PlaceLamp", () => OnPlaceLamp(x, y));
            _inputHandler.RemoveObject        += (x, y) => SafeCall("RemoveObject", () => OnRemoveObject(x, y));
            _inputHandler.SwitchHand          += () => SafeCall("SwitchHand", () => _activeHandIndex = 1 - _activeHandIndex);
            _inputHandler.SelectSlot          += (i) => SafeCall("SelectSlot", () => _activeSlotIndex = i);
            _inputHandler.PlaceInPocket       += () => SafeCall("PlaceInPocket", OnPlaceInPocket);
            _inputHandler.UseItem             += () => SafeCall("UseItem", OnUseItem);
            _inputHandler.DropItem            += () => SafeCall("DropItem", OnDropItem);
            _inputHandler.ModifyOxygen        += (x, y, d) => SafeCall("ModifyOxygen", () => OnModifyOxygen(x, y, d));
            _inputHandler.ModifyNitrogen      += (x, y, d) => SafeCall("ModifyNitrogen", () => OnModifyNitrogen(x, y, d));
            _inputHandler.ModifyCarbonDioxide += (x, y, d) => SafeCall("ModifyCO2", () => OnModifyCarbonDioxide(x, y, d));
            _inputHandler.ModifyTemperature   += (x, y, d) => SafeCall("ModifyTemp", () => OnModifyTemperature(x, y, d));
            _inputHandler.ModifyPlasma        += (x, y, d) => SafeCall("ModifyPlasma", () => OnModifyPlasma(x, y, d));
        }

        private void SafeCall(string system, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                _gameState.Errors.Report(system, ex);
                Logger.Error(system, $"Runtime error caught: {ex.Message}", ex);
            }
        }

        // ===== Взаимодействие с миром =====

        private void OnLeftClick(int tx, int ty)
        {
            if (_gameState.Map.GetTile(tx, ty) == TileType.Door)
            {
                ToggleDoorGroup(tx, ty);
            }
            else
            {
                PickupItemAt(tx, ty);
            }
        }

        private void ToggleDoorGroup(int x, int y)
        {
            int gid = _gameState.Map.GetDoorGroupId(x, y);
            if (gid <= 0) return;

            bool open = _gameState.Map.IsDoorOpen(x, y);
            bool newState = !open;

            _gameState.Map.SetDoorOpen(x, y, newState);
            var tiles = _gameState.Map.GetDoorTilesInGroup(gid);
            foreach (var (dx, dy) in tiles)
                _gameState.Atmosphere.SetDoorState(dx, dy, newState);

            _hudRenderer.Notify(newState ? "Дверь открыта" : "Дверь закрыта",
                new Color(1f, 0.85f, 0.4f, 1f));
        }

        private void PickupItemAt(int x, int y)
        {
            var items = _gameState.Map.GetItemsAtTile(x, y);
            if (items == null || items.Count == 0) return;

            var item = items[0];
            _gameState.Map.RemoveItemAtTile(x, y, item);
            if (!_gameState.Player.Inventory.AddToHand(item) &&
                !_gameState.Player.Inventory.AddToPocket(item) &&
                !_gameState.Player.Inventory.AddToBelt(item) &&
                !_gameState.Player.Inventory.AddToBack(item))
                _gameState.Map.AddItemToTile(x, y, item);
            else
                _hudRenderer.Notify($"Подобрано: {item.Name}", new Color(0.6f, 0.9f, 1f, 1f));
        }

        private void OnPlaceWall(int x, int y)
        {
            var tile = _gameState.Map.GetTile(x, y);
            if (tile == TileType.Wall || tile == TileType.ReinforcedWall || tile == TileType.Door)
                return;

            _gameState.Map.SetTile(x, y, TileType.Wall);
            _gameState.Atmosphere.SetWall(x, y, true);
            _hudRenderer.Notify($"Стена поставлена ({x},{y})");
        }

        private void OnPlaceDoor(int x, int y)
        {
            float dirX = MathF.Cos(_gameState.Player.Direction);
            float dirY = MathF.Sin(_gameState.Player.Direction);
            bool facingEastWest = MathF.Abs(dirX) > MathF.Abs(dirY);

            for (int i = 0; i < DoorWidth; i++)
            {
                int tx = facingEastWest ? x : x + i;
                int ty = facingEastWest ? y + i : y;
                if (!CanPlaceDoorTile(tx, ty))
                {
                    _hudRenderer.Notify($"Нельзя поставить дверь ({tx},{ty})", new Color(1f, 0.5f, 0.5f, 1f));
                    return;
                }
            }

            int gid = _gameState.Map.NewDoorGroup();
            for (int i = 0; i < DoorWidth; i++)
            {
                int tx = facingEastWest ? x : x + i;
                int ty = facingEastWest ? y + i : y;
                PlaceDoorTile(tx, ty, gid);
                _gameState.Atmosphere.SetDoorState(tx, ty, false);
            }

            _hudRenderer.Notify($"Дверь установлена ({x},{y})", new Color(0.6f, 1f, 0.6f, 1f));
        }

        private bool CanPlaceDoorTile(int x, int y)
        {
            if (x < 0 || x >= _gameState.Map.Width || y < 0 || y >= _gameState.Map.Height) return false;
            var t = _gameState.Map.GetTile(x, y);
            return t != TileType.Wall && t != TileType.ReinforcedWall && t != TileType.Door;
        }

        private void PlaceDoorTile(int x, int y, int groupId)
        {
            var obj = ObjectFactory.Create(TileType.Door);
            if (obj != null)
            {
                _gameState.Map.SetTile(x, y, obj.TileType);
                obj.OnPlaced(_gameState.Map, x, y);
            }
            else
            {
                _gameState.Map.SetTile(x, y, TileType.Door);
            }
            _gameState.Map.RegisterDoorTile(x, y, groupId);
        }

        private void OnPlaceWire(int x, int y) { SetTile(x, y, TileType.Wire); _hudRenderer.Notify($"Провод ({x},{y})"); }
        private void OnPlaceGenerator(int x, int y) { SetTile(x, y, TileType.Generator); _hudRenderer.Notify($"Генератор ({x},{y})", new Color(0.9f, 0.8f, 0.4f, 1f)); }
        private void OnPlaceLamp(int x, int y) { SetTile(x, y, TileType.Lamp); _hudRenderer.Notify($"Лампа ({x},{y})", new Color(0.9f, 0.9f, 0.5f, 1f)); }

        private void OnRemoveObject(int x, int y)
        {
            var tile = _gameState.Map.GetTile(x, y);
            if (tile == TileType.Floor)
            {
                _gameState.Map.SetTile(x, y, TileType.Space);
                _gameState.Atmosphere.SetTileToSpace(x, y);
                _hudRenderer.Notify($"Убрано в Space ({x},{y})", new Color(0.8f, 0.8f, 1f, 1f));
            }
            else if (tile == TileType.Space)
            {
                _gameState.Map.SetTile(x, y, TileType.Floor);
                _gameState.Atmosphere.SetTileToFloor(x, y);
                _hudRenderer.Notify($"Восстановлен пол ({x},{y})", new Color(0.7f, 1f, 0.7f, 1f));
            }
            else
            {
                _gameState.Map.SetTile(x, y, TileType.Floor);
                _gameState.Atmosphere.SetTileToFloor(x, y);
                _hudRenderer.Notify($"Удалено ({x},{y})", new Color(1f, 0.7f, 0.7f, 1f));
            }
        }

        private void SetTile(int x, int y, TileType type)
        {
            if (_gameState.Map.GetTile(x, y) != TileType.Wall &&
                _gameState.Map.GetTile(x, y) != TileType.ReinforcedWall &&
                _gameState.Map.GetTile(x, y) != TileType.Door)
            {
                var obj = ObjectFactory.Create(type);
                if (obj != null)
                {
                    _gameState.Map.SetTile(x, y, obj.TileType);
                    obj.OnPlaced(_gameState.Map, x, y);
                }
            }
        }

        private void OnPlaceInPocket()
        {
            int h = _activeHandIndex;
            var item = _gameState.Player.Inventory.RemoveItem(h);
            if (item != null)
            {
                if (!_gameState.Player.Inventory.AddToPocket(item) &&
                    !_gameState.Player.Inventory.AddToBelt(item) &&
                    !_gameState.Player.Inventory.AddToBack(item))
                    _gameState.Player.Inventory.SetItem(h, item);
                else
                    _hudRenderer.Notify($"В карман: {item.Name}");
            }
        }

        private void OnUseItem()
        {
            int h = _activeHandIndex;
            var item = _gameState.Player.Inventory.GetItem(h);
            if (item != null && _gameState.Player.Inventory.UseItem(h, _gameState.Player))
                _hudRenderer.Notify($"Использовано: {item.Name}", new Color(0.7f, 1f, 0.7f, 1f));
        }

        private void OnDropItem()
        {
            int h = _activeHandIndex;
            var item = _gameState.Player.Inventory.RemoveItem(h);
            if (item != null)
            {
                int tx = (int)(_gameState.Player.Position.X / _gameState.Map.TileSize);
                int ty = (int)(_gameState.Player.Position.Y / _gameState.Map.TileSize);
                _gameState.Map.AddItemToTile(tx, ty, item);
                _hudRenderer.Notify($"Выброшено: {item.Name}", new Color(1f, 0.85f, 0.6f, 1f));
            }
        }

        private void OnModifyOxygen(int x, int y, float d) => _gameState.Atmosphere.ModifyOxygen(x, y, d);
        private void OnModifyNitrogen(int x, int y, float d) => _gameState.Atmosphere.ModifyNitrogen(x, y, d);
        private void OnModifyCarbonDioxide(int x, int y, float d) => _gameState.Atmosphere.ModifyCarbonDioxide(x, y, d);
        private void OnModifyTemperature(int x, int y, float d) => _gameState.Atmosphere.ModifyTemperature(x, y, d);
        private void OnModifyPlasma(int x, int y, float d) => _gameState.Atmosphere.ModifyPlasma(x, y, d);

        private void UpdateCamera3D()
        {
            const float TileSizeF = 32f;
            const float EyeHeight = 1.6f;

            _camera3D.Position = new Vector3(
                _gameState.Player.Position.X / TileSizeF,
                EyeHeight,
                _gameState.Player.Position.Y / TileSizeF
            );
            _camera3D.Yaw = _gameState.Player.Direction;
            _camera3D.Pitch = _gameState.Player.Pitch;
        }

        // ===== Меню взаимодействий =====

        private void UpdateInteractionMenu()
        {
            bool altDown = _keyboard.IsKeyPressed(Key.AltLeft) || _keyboard.IsKeyPressed(Key.AltRight);
            bool rmbDown = _mouse.IsButtonPressed(MouseButton.Right);

            // Отпускание курсора при зажатии Alt
            if (altDown && !_wasAltDown)
            {
                SetCursorMode(CursorMode.Normal);
                _inputHandler.ResetMouseTracking();
                Logger.Info("InteractionMenu", "Cursor released (Alt down)");
            }

            // Захват курсора при отпускании Alt
            if (!altDown && _wasAltDown)
            {
                SetCursorMode(CursorMode.Raw);
                _inputHandler.ResetMouseTracking();
                Logger.Info("InteractionMenu", "Cursor captured (Alt up)");
            }

            // Открытие меню — Alt + ПКМ
            if (altDown && rmbDown && !_interactionMenu.IsOpen)
            {
                BuildInteractionMenu();
                if (_interactionMenu.IsOpen)
                    Logger.Info("InteractionMenu", $"Opened: {_interactionMenu.Title} ({_interactionMenu.Items.Count} items)");
            }

            // Hover + клик по пункту — только когда меню открыто
            if (_interactionMenu.IsOpen)
            {
                float mx = _mouse.Position.X;
                float my = _mouse.Position.Y;

                _interactionMenu.HoveredIndex = InteractionMenuRenderer.HitTest(
                    _interactionMenu, mx, my, _actualWidth, _actualHeight);

                bool lmb = _mouse.IsButtonPressed(MouseButton.Left);
                if (lmb && !_lmbInMenuPrev)
                {
                    var hovered = _interactionMenu.GetHovered();
                    if (hovered != null)
                    {
                        try { hovered.Execute(); }
                        catch (Exception ex)
                        {
                            _gameState.Errors.Report("InteractionMenu", ex);
                            Logger.Error("InteractionMenu", $"Action failed: {ex.Message}", ex);
                        }
                    }
                    _interactionMenu.Clear();
                }
                _lmbInMenuPrev = lmb;
            }
            else
            {
                _lmbInMenuPrev = false;
            }

            // Меню закрывается при отпускании Alt
            if (!altDown && _interactionMenu.IsOpen)
            {
                _interactionMenu.Clear();
                Logger.Info("InteractionMenu", "Closed (Alt up)");
            }

            // Блокируем ввод, пока Alt зажат
            _inputHandler.Blocked = altDown;

            _wasAltDown = altDown;
        }

        private void BuildInteractionMenu()
        {
            _interactionMenu.Clear();

            if (!_currentHit.Hit) return;

            int x = _currentHit.X;
            int y = _currentHit.Z;
            var tile = _gameState.Map.GetTile(x, y);

            _interactionMenu.Title = $"{GetTileName(tile)} ({x},{y})";

            switch (tile)
            {
                case TileType.Door:
                    _interactionMenu.Add("Открыть / Закрыть", () => ToggleDoorGroup(x, y));
                    _interactionMenu.Add("Удалить", () => SafeCall("MenuRemove", () => OnRemoveObject(x, y)));
                    break;

                case TileType.Wall:
                case TileType.ReinforcedWall:
                    _interactionMenu.Add("Удалить стену", () => SafeCall("MenuRemove", () => OnRemoveObject(x, y)));
                    break;

                case TileType.Floor:
                    _interactionMenu.Add("Построить стену",        () => SafeCall("MenuWall",   () => OnPlaceWall(x, y)));
                    _interactionMenu.Add("Построить дверь",        () => SafeCall("MenuDoor",   () => OnPlaceDoor(x, y)));
                    _interactionMenu.Add("Проложить провод",       () => SafeCall("MenuWire",   () => OnPlaceWire(x, y)));
                    _interactionMenu.Add("Поставить генератор",    () => SafeCall("MenuGen",    () => OnPlaceGenerator(x, y)));
                    _interactionMenu.Add("Поставить лампу",        () => SafeCall("MenuLamp",   () => OnPlaceLamp(x, y)));
                    _interactionMenu.Add("Убрать в космос",        () => SafeCall("MenuRemove", () => OnRemoveObject(x, y)));
                    break;

                case TileType.Space:
                    _interactionMenu.Add("Восстановить пол", () => SafeCall("MenuRestore", () => OnRemoveObject(x, y)));
                    break;

                case TileType.Wire:
                case TileType.Lamp:
                case TileType.Generator:
                    _interactionMenu.Add("Удалить", () => SafeCall("MenuRemove", () => OnRemoveObject(x, y)));
                    break;
            }

            var items = _gameState.Map.GetItemsAtTile(x, y);
            if (items != null && items.Count > 0)
            {
                string label = items.Count == 1
                    ? $"Подобрать: {items[0].Name}"
                    : $"Подобрать (первый: {items[0].Name})";
                _interactionMenu.Add(label, () => PickupItemAt(x, y));
            }
        }

        private void SetCursorMode(CursorMode mode)
        {
            try { _mouse.Cursor.CursorMode = mode; } catch { }
        }

        // ===== Update / Render =====

        protected override void OnUpdate(double deltaTime)
        {
            var (p, a, pw, h) = _gameState.Update((float)deltaTime);
            _playerUpdateMs = p;
            _atmosUpdateMs = a;
            _powerUpdateMs = pw;
            _healthUpdateMs = h;

            UpdateCamera3D();

            Vector3 eye = _camera3D.Position;
            Vector3 dir = _camera3D.Forward;
            _currentHit = Raycaster.Cast(_gameState.Map, eye, dir, ReachDistance);

            UpdateInteractionMenu();

            _inputHandler.Update((float)deltaTime, _currentHit);

            HandleHotkeys();

            _hudRenderer.Update((float)deltaTime);
        }

        private void HandleHotkeys()
        {
            // P — сохранить
            bool save = _keyboard.IsKeyPressed(Key.P);
            if (save && !_savePrev)
            {
                try
                {
                    SaveManager.Save(_gameState.Map, _gameState.Player, SaveManager.DefaultPath);
                    _hudRenderer.Notify($"Станция сохранена: {System.IO.Path.GetFileName(SaveManager.DefaultPath)}",
                        new Color(0.7f, 1f, 0.7f, 1f));
                }
                catch (Exception ex)
                {
                    _gameState.Errors.Report("Save", ex);
                    _hudRenderer.Notify($"Ошибка сохранения: {ex.Message}", new Color(1f, 0.5f, 0.5f, 1f));
                }
            }
            _savePrev = save;

            // Home — загрузить
            bool load = _keyboard.IsKeyPressed(Key.Home);
            if (load && !_loadPrev)
            {
                try
                {
                    if (!SaveManager.Exists(SaveManager.DefaultPath))
                    {
                        _hudRenderer.Notify("Сохранение не найдено (P — сохранить)", new Color(1f, 0.85f, 0.5f, 1f));
                    }
                    else
                    {
                        var data = SaveManager.Load(SaveManager.DefaultPath);
                        SaveManager.Apply(data, _gameState.Map, _gameState.Player);
                        _gameState.Atmosphere.Initialize(_gameState.Map);
                        _gameState.Power.Initialize(_gameState.Map);
                        _hudRenderer.Notify("Станция загружена", new Color(0.7f, 1f, 0.7f, 1f));
                    }
                }
                catch (Exception ex)
                {
                    _gameState.Errors.Report("Load", ex);
                    _hudRenderer.Notify($"Ошибка загрузки: {ex.Message}", new Color(1f, 0.5f, 0.5f, 1f));
                }
            }
            _loadPrev = load;

            // Tab — debug
            bool debug = _keyboard.IsKeyPressed(Key.Tab);
            if (debug && !_debugPrev)
                _hudRenderer.DebugVisible = !_hudRenderer.DebugVisible;
            _debugPrev = debug;

            // Escape — панель ошибок
            bool errPanel = _keyboard.IsKeyPressed(Key.Escape);
            if (errPanel && !_errPanelPrev)
                _hudRenderer.ErrorPanelVisible = !_hudRenderer.ErrorPanelVisible;
            _errPanelPrev = errPanel;

            // Delete — очистка
            bool clearErr = _keyboard.IsKeyPressed(Key.Delete);
            if (clearErr && !_clearErrPrev)
            {
                _gameState.Errors.Clear();
                _hudRenderer.Notify("Список ошибок очищен", new Color(0.7f, 1f, 0.7f, 1f));
            }
            _clearErrPrev = clearErr;
        }

        protected override void OnRender(double deltaTime)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            _framebuffer.Bind();
            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Less);
            Gl.DepthMask(true);
            Gl.ClearColor(0.05f, 0.05f, 0.08f, 1f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            try { _world3D.Draw(_camera3D, _currentHit); }
            catch (Exception ex)
            {
                _gameState.Errors.Report("World3D.Draw", ex);
                Logger.Error("World3D.Draw", "Runtime error caught", ex);
            }

            sw.Stop();
            _renderMs = (float)sw.Elapsed.TotalMilliseconds;

            _framebuffer.Unbind();
            _framebuffer.BlitToScreen((int)_actualWidth, (int)_actualHeight);

            Gl.Viewport(0, 0, (uint)_actualWidth, (uint)_actualHeight);
            Gl.Disable(EnableCap.DepthTest);

            string hoverName = _currentHit.Hit ? GetTileName(_currentHit.Tile) : "";
            TileType hoverTile = _currentHit.Hit ? _currentHit.Tile : TileType.Floor;

            _hudRenderer.Draw(
                _fps,
                _playerUpdateMs, _atmosUpdateMs, _powerUpdateMs, _healthUpdateMs, _renderMs,
                _activeSlotIndex, _activeHandIndex,
                _actualWidth, _actualHeight,
                hoverName,
                _currentHit.Hit, hoverTile,
                _currentHit.X, _currentHit.Z,
                _gameState.Runtime,
                _gameState.Errors,
                _interactionMenu);

            _gameState.Runtime.NextFrame();
            _frameCount++;
            if (_fpsTimer.Elapsed.TotalSeconds >= 1.0)
            {
                _fps = _frameCount / (float)_fpsTimer.Elapsed.TotalSeconds;
                _frameCount = 0;
                _fpsTimer.Restart();
                _gameState.Runtime.SetFps(_fps);

                Logger.Info("Perf",
                    $"FPS={_fps:F1} Player={_playerUpdateMs:F2}ms Atmos={_atmosUpdateMs:F2}ms " +
                    $"Render={_renderMs:F2}ms Tiles={_world3D.LastDrawnTiles} " +
                    $"Uptime={_gameState.Runtime.FormatRealTime()} Errors={_gameState.Errors.TotalCount}");
            }
        }

        private static string GetTileName(TileType tile) => tile switch
        {
            TileType.Floor          => "Floor",
            TileType.Wall           => "Wall",
            TileType.ReinforcedWall => "Reinforced Wall",
            TileType.Wire           => "Wire",
            TileType.Generator      => "Generator",
            TileType.Lamp           => "Lamp",
            TileType.Door           => "Door",
            TileType.Space          => "Space",
            _                       => tile.ToString()
        };
    }
}