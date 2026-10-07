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
using Framebuffer = SS15.Client.Render.Framebuffer;   // ← алиас обязателен

namespace SS15.Client
{
    public class MainGame : GraphicsDevice
    {
        private SpriteBatch _spriteBatch;
        private IInputContext _inputContext;   // держим живым, иначе GC отберёт ввод
        private IKeyboard _keyboard;
        private IMouse _mouse;

        private GameState _gameState;
        private Camera3D _camera3D;
        private WorldRenderer3D _world3D;
        private HudRenderer _hudRenderer;
        private InputHandler _inputHandler;
        private Framebuffer _framebuffer;

        // Текстуры
        private Texture2D _floorTex, _spaceTex;
        private Texture2D _wallSingleTex, _wallNSTex, _wallWETex;
        private Texture2D _gridTex, _playerTex, _itemTex, _whiteTexture, _slotTexture;
        private Texture2D _generatorTex, _lampOnTex, _lampOffTex;
        private Texture2D _doorOpenTex, _doorClosedTex;
        private Texture2D _armsPreviewTex, _pocketTex, _beltTex, _backTex;

        private System.Diagnostics.Stopwatch _fpsTimer;
        private int _frameCount;
        private float _fps;
        private float _playerUpdateMs, _atmosUpdateMs, _powerUpdateMs, _healthUpdateMs, _renderMs;

        private int _activeSlotIndex = 0;
        private int _activeHandIndex = 0;

        private float _actualWidth, _actualHeight;
        private Vector2 _windowScale;

        private DateTime _lastCamLog = DateTime.MinValue;

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

            // === FBO с depth ===
            _framebuffer = new Framebuffer(Gl, (int)_actualWidth, (int)_actualHeight);

            _fpsTimer = System.Diagnostics.Stopwatch.StartNew();

            // ВАЖНО: inputContext храним в поле, иначе GC может отобрать ввод
            _inputContext = Window.CreateInput();
            _keyboard = _inputContext.Keyboards[0];
            _mouse = _inputContext.Mice[0];
            Logger.Info("MainGame", "Input initialized");

            _gameState = new GameState();
            _gameState.Initialize(_keyboard);
            Logger.Info("MainGame", $"GameState initialized. Map: {_gameState.Map.Width}x{_gameState.Map.Height}");
            Logger.Info("MainGame", $"Player spawn: {_gameState.Player.Position}");

<<<<<<< Updated upstream
            // Рендерер мира
            _worldRenderer = new WorldRenderer(_spriteBatch, _camera, _gameState.Map, _gameState.Player, _gameState.Power);
            _worldRenderer.SetTextures(
                _floorTex, _spaceTex, _wallSingleTex,
                _wallNorthSouthTex, _wallWestEastTex,
                _wallEastSouthTex, _wallNorthEastTex, _wallSouthWestTex, _wallWestNorthTex,
                _gridTex, _playerTex, _itemTex,
                _whiteTexture,
                _generatorTex, _lampOnTex, _lampOffTex,
                _doorOpenTex, _doorClosedTex);
=======
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
>>>>>>> Stashed changes

            _world3D.SetTextures(
                _wallSingleTex, _wallNSTex, _wallWETex,
                _floorTex, _spaceTex,
                _doorOpenTex, _doorClosedTex,
                _generatorTex, _lampOnTex, _lampOffTex);

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

            _inputHandler = new InputHandler(_keyboard, _mouse, _gameState.Map, _gameState.Player);
            SubscribeInputEvents();

            _gameState.Player.Direction = 0f;
            _gameState.Player.Pitch = 0f;

            Logger.Info("MainGame", "OnLoad complete");
        }

        private void LoadAllTextures()
        {
            try
            {
                _floorTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Floors/floor.png", (gl) => SpriteGenerator.CreateFloorTexture(gl, 32));
                Logger.Info("Tex", $"Floor loaded, handle={_floorTex?.Handle ?? 0}");

                _spaceTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/space.png", (gl) => SpriteGenerator.CreateSpaceTexture(gl, 32));
                Logger.Info("Tex", $"Space loaded, handle={_spaceTex?.Handle ?? 0}");

                _wallSingleTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Walls/Wall_METAL/Wall_METAL_SINGLE.png", (gl) => SpriteGenerator.CreateWallTexture(gl, 32));
                Logger.Info("Tex", $"Wall single, handle={_wallSingleTex?.Handle ?? 0}");

                _wallNSTex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Walls/Wall_METAL/Directions/Wall_METAL_North_South.png", (gl) => SpriteGenerator.CreateWallTexture(gl, 32));
                Logger.Info("Tex", $"Wall NS, handle={_wallNSTex?.Handle ?? 0}");

                _wallWETex = SpriteGenerator.LoadTexture(Gl, "Sprites/Tiles/Walls/Wall_METAL/Directions/Wall_METAL_West_East.png", (gl) => SpriteGenerator.CreateWallTexture(gl, 32));
                Logger.Info("Tex", $"Wall WE, handle={_wallWETex?.Handle ?? 0}");

                _playerTex = SpriteGenerator.LoadTexture(Gl, "Sprites/player.png", (gl) => SpriteGenerator.CreatePlayerTexture(gl, 32));

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
            {
                _framebuffer.Resize(size.X, size.Y);
            }

            Logger.Info("MainGame", $"Window resized to {size.X}x{size.Y}");
        }

        private void SubscribeInputEvents()
        {
            _inputHandler.LeftClick += OnLeftClick;
            _inputHandler.PlaceWall += OnPlaceWall;
            _inputHandler.PlaceDoor += OnPlaceDoor;
            _inputHandler.PlaceWire += OnPlaceWire;
            _inputHandler.PlaceGenerator += OnPlaceGenerator;
            _inputHandler.PlaceLamp += OnPlaceLamp;
            _inputHandler.RemoveObject += OnRemoveObject;
            _inputHandler.SwitchHand += () => _activeHandIndex = 1 - _activeHandIndex;
            _inputHandler.SelectSlot += (i) => _activeSlotIndex = i;
            _inputHandler.PlaceInPocket += OnPlaceInPocket;
            _inputHandler.UseItem += OnUseItem;
            _inputHandler.DropItem += OnDropItem;
            _inputHandler.ModifyOxygen += OnModifyOxygen;
            _inputHandler.ModifyNitrogen += OnModifyNitrogen;
            _inputHandler.ModifyCarbonDioxide += OnModifyCarbonDioxide;
            _inputHandler.ModifyTemperature += OnModifyTemperature;
        }

        private void OnLeftClick(int tx, int ty)
        {
            if (_gameState.Map.GetTile(tx, ty) == TileType.Door)
            {
                bool open = _gameState.Map.IsDoorOpen(tx, ty);
                _gameState.Map.SetDoorOpen(tx, ty, !open);
                _gameState.Atmosphere.SetDoorState(tx, ty, !open);
            }
            else
            {
                var items = _gameState.Map.GetItemsAtTile(tx, ty);
                if (items != null && items.Count > 0)
                {
                    var item = items[0];
                    _gameState.Map.RemoveItemAtTile(tx, ty, item);
                    if (!_gameState.Player.Inventory.AddToHand(item) &&
                        !_gameState.Player.Inventory.AddToPocket(item) &&
                        !_gameState.Player.Inventory.AddToBelt(item) &&
                        !_gameState.Player.Inventory.AddToBack(item))
                        _gameState.Map.AddItemToTile(tx, ty, item);
                }
            }
        }

        // ИСПРАВЛЕНО: ставим стену и в Map, и в Atmosphere
        private void OnPlaceWall(int x, int y)
        {
            var tile = _gameState.Map.GetTile(x, y);
            if (tile == TileType.Wall ||
                tile == TileType.ReinforcedWall ||
                tile == TileType.Door)
                return;

            _gameState.Map.SetTile(x, y, TileType.Wall);
            _gameState.Atmosphere.SetWall(x, y, true);
        }

        private void OnPlaceDoor(int x, int y) { SetTile(x, y, TileType.Door); _gameState.Atmosphere.SetDoorState(x, y, false); }
        private void OnPlaceWire(int x, int y) => SetTile(x, y, TileType.Wire);
        private void OnPlaceGenerator(int x, int y) => SetTile(x, y, TileType.Generator);
        private void OnPlaceLamp(int x, int y) => SetTile(x, y, TileType.Lamp);

        private void OnRemoveObject(int x, int y)
        {
            var tile = _gameState.Map.GetTile(x, y);
            if (tile == TileType.Floor)
            {
                _gameState.Map.SetTile(x, y, TileType.Space);
                _gameState.Atmosphere.SetTileToSpace(x, y);
            }
            else if (tile == TileType.Space)
            {
                _gameState.Map.SetTile(x, y, TileType.Floor);
                _gameState.Atmosphere.SetTileToFloor(x, y);
            }
            else
            {
                _gameState.Map.SetTile(x, y, TileType.Floor);
                _gameState.Atmosphere.SetTileToFloor(x, y);
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
            }
        }

        private void OnUseItem()
        {
            int h = _activeHandIndex;
            var item = _gameState.Player.Inventory.GetItem(h);
            if (item != null) _gameState.Player.Inventory.UseItem(h, _gameState.Player);
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
            }
        }

<<<<<<< Updated upstream
        private void OnModifyOxygen(int x, int y, float delta) => _gameState.Atmosphere.ModifyOxygen(x, y, delta);
        private void OnModifyNitrogen(int x, int y, float delta) => _gameState.Atmosphere.ModifyNitrogen(x, y, delta);
        private void OnModifyCarbonDioxide(int x, int y, float delta) => _gameState.Atmosphere.ModifyCarbonDioxide(x, y, delta);
        private void OnModifyTemperature(int x, int y, float delta) => _gameState.Atmosphere.ModifyTemperature(x, y, delta);
=======
        private void OnModifyOxygen(int x, int y, float d) => _gameState.Atmosphere.ModifyOxygen(x, y, d);
        private void OnModifyNitrogen(int x, int y, float d) => _gameState.Atmosphere.ModifyNitrogen(x, y, d);
        private void OnModifyCarbonDioxide(int x, int y, float d) => _gameState.Atmosphere.ModifyCarbonDioxide(x, y, d);
        private void OnModifyTemperature(int x, int y, float d) => _gameState.Atmosphere.ModifyTemperature(x, y, d);
        private void OnModifyPlasma(int x, int y, float d) => _gameState.Atmosphere.ModifyPlasma(x, y, d);
>>>>>>> Stashed changes

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

            if ((DateTime.UtcNow - _lastCamLog).TotalSeconds >= 1.0)
            {
                _lastCamLog = DateTime.UtcNow;
                int tx = (int)_camera3D.Position.X;
                int tz = (int)_camera3D.Position.Z;
                var tile = _gameState.Map.GetTile(tx, tz);
                Logger.Trace("Camera",
                    $"Pos=({_camera3D.Position.X:F2},{_camera3D.Position.Y:F2},{_camera3D.Position.Z:F2}) " +
                    $"Yaw={_camera3D.Yaw:F2} Pitch={_camera3D.Pitch:F2} " +
                    $"Tile=({tx},{tz})={tile}");
            }
        }

        protected override void OnUpdate(double deltaTime)
        {
<<<<<<< Updated upstream
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _gameState.Update((float)deltaTime);
            _playerUpdateMs = (float)sw.Elapsed.TotalMilliseconds;
=======
            var (p, a, pw, h) = _gameState.Update((float)deltaTime);
            _playerUpdateMs = p;
            _atmosUpdateMs = a;
            _powerUpdateMs = pw;
            _healthUpdateMs = h;
>>>>>>> Stashed changes

            _inputHandler.Update((float)deltaTime);
            UpdateCamera3D();
        }

        protected override void OnRender(double deltaTime)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // === 1. 3D в FBO (с depth) ===
            _framebuffer.Bind();

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Less);
            Gl.DepthMask(true);
            Gl.ClearColor(0.05f, 0.05f, 0.08f, 1f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _world3D.Draw(_camera3D);

            // Замеряем сразу после отрисовки мира, чтобы HUD в этом же кадре
            // получил актуальное значение, а не значение прошлого кадра
            sw.Stop();
            _renderMs = (float)sw.Elapsed.TotalMilliseconds;

            _framebuffer.Unbind();

            // === 2. Blit FBO -> default framebuffer ===
            _framebuffer.BlitToScreen((int)_actualWidth, (int)_actualHeight);

            // === 3. HUD поверх ===
            Gl.Viewport(0, 0, (uint)_actualWidth, (uint)_actualHeight);
            Gl.Disable(EnableCap.DepthTest);

            _hudRenderer.Draw(
                _fps,
                _playerUpdateMs, _atmosUpdateMs, _powerUpdateMs, _healthUpdateMs, _renderMs,
                _activeSlotIndex, _activeHandIndex,
                _actualWidth, _actualHeight);

            _frameCount++;
            if (_fpsTimer.Elapsed.TotalSeconds >= 1.0)
            {
                _fps = _frameCount / (float)_fpsTimer.Elapsed.TotalSeconds;
                _frameCount = 0;
                _fpsTimer.Restart();

                Logger.Info("Perf",
                    $"FPS={_fps:F1} Player={_playerUpdateMs:F2}ms Atmos={_atmosUpdateMs:F2}ms " +
                    $"Render={_renderMs:F2}ms Tiles={_world3D.LastDrawnTiles}");
            }
        }
    }
}