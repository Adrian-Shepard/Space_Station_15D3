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
using SS15.Client.Render.Models;
using SS15.Client.Systems;
using Framebuffer = SS15.Client.Render.Framebuffer;

namespace SS15.Client
{
    public class MainGame : GraphicsDevice
    {
        private GameContext _ctx;
        private AssetLibrary _assets;

        private WorldInteractionSystem _worldInteraction;
        private InteractionMenuSystem _interactionMenu;
        private HotkeySystem _hotkeys;
        private RenderPipeline _pipeline;

        private IInputContext _inputContext;

        private System.Diagnostics.Stopwatch _fpsTimer;
        private int _frameCount;
        private float _fps;

        private Vector2 _lastPlayerPos;
        private Vector2 _windowScale;

        private const float ReachDistance = 5.0f;

        public MainGame() : base(800, 600, "Space Station 15 — 3D")
        {
        }

        protected override void OnLoad()
        {
            Logger.Info("MainGame", "OnLoad started");

            _assets = AssetLibrary.Load(Gl);

            _ctx = new GameContext
            {
                Gl = Gl,
                Window = Window,
                ScreenWidth = Window.Size.X,
                ScreenHeight = Window.Size.Y,
                Assets = _assets,
                Menu = new InteractionMenu()
            };

            _ctx.SpriteBatch = new SpriteBatch(Gl);

            LogOpenGLInfo();

            Gl.Viewport(0, 0, (uint)_ctx.ScreenWidth, (uint)_ctx.ScreenHeight);
            UpdateWindowScale();

            _ctx.Framebuffer = new Framebuffer(Gl, (int)_ctx.ScreenWidth, (int)_ctx.ScreenHeight);
            _fpsTimer = System.Diagnostics.Stopwatch.StartNew();

            _inputContext = Window.CreateInput();
            _ctx.Keyboard = _inputContext.Keyboards[0];
            _ctx.Mouse = _inputContext.Mice[0];
            Logger.Info("MainGame", "Input initialized");

            _ctx.State = new GameState();
            _ctx.State.Initialize(_ctx.Keyboard);
            _lastPlayerPos = _ctx.State.Player.Position;

            _ctx.Camera = new Camera3D
            {
                ViewportWidth = (int)_ctx.ScreenWidth,
                ViewportHeight = (int)_ctx.ScreenHeight
            };

            var playerModelPath = System.IO.Path.Combine(
                AppContext.BaseDirectory, "Assets", "Models", "player.glb");
            var playerModel = ModelLoader.Load(Gl, playerModelPath, targetHeight: 2.0f);

            _ctx.World = new WorldRenderer3D(
                Gl,
                _ctx.State.Map,
                _ctx.State.Power,
                _ctx.State.Atmosphere,
                _ctx.State.Physics,
                _ctx.State.ItemFlight,
                _ctx.State.RemotePlayers,
                _assets.White,
                playerModel);

            _ctx.World.SetTextures(
                _assets.WallSingle, _assets.WallNS, _assets.WallWE,
                _assets.Floor, _assets.Space,
                _assets.Wire,
                _assets.DoorOpen, _assets.DoorClosed,
                _assets.Generator, _assets.LampOn, _assets.LampOff,
                _assets.Highlight,
                _assets.Crate,
                _assets.Item);

            _ctx.ViewModel = new ViewModelRenderer(Gl, _assets.Hand, _assets.Item);

            var hudCamera = new Camera2D
            {
                ViewportWidth = 800,
                ViewportHeight = 600,
                Zoom = 1f,
                IsoYFactor = 1f,
                TileHeight = 0f
            };

            _ctx.Hud = new HudRenderer(
                _ctx.SpriteBatch, hudCamera, _ctx.Mouse,
                _ctx.State.Player, _ctx.State.Atmosphere, _ctx.State.Power,
                new FontRenderer(Gl), _assets.Slot, _assets.White, _assets.Item,
                _assets.ArmsPreview, _assets.Pocket, _assets.Belt, _assets.Back);

            _ctx.State.Errors.OnNewError += OnRuntimeError;

            _ctx.Input = new InputHandler(_ctx.Keyboard, _ctx.Mouse, _ctx.State.Map, _ctx.State.Player);

            _worldInteraction = new WorldInteractionSystem(_ctx);
            _interactionMenu = new InteractionMenuSystem(_ctx, _worldInteraction);
            _hotkeys = new HotkeySystem(_ctx);
            _pipeline = new RenderPipeline(_ctx);

            SubscribeInputEvents();

            _ctx.State.Player.Direction = 0f;
            _ctx.State.Player.Pitch = 0f;

            _ctx.Hud.Notify("Добро пожаловать на борт, инженер.", new Color(1f, 0.9f, 0.6f, 1f));
            _ctx.Hud.Notify("Alt — отпустить курсор, Alt+ПКМ — меню", new Color(0.8f, 1f, 0.8f, 1f));
            _ctx.Hud.Notify("F — тащить ящик, Shift — спринт, Space — bhop", new Color(0.8f, 1f, 0.8f, 1f));
            _ctx.Hud.Notify("Ctrl+Q — бросить предмет", new Color(0.8f, 1f, 0.8f, 1f));

            Logger.Info("MainGame", "OnLoad complete");
        }

        private void SubscribeInputEvents()
        {
            _ctx.Input.LeftClick           += (x, y) => _worldInteraction.SafeCall("LeftClick",   () => _worldInteraction.LeftClick(x, y));
            _ctx.Input.PlaceWall           += (x, y) => _worldInteraction.SafeCall("PlaceWall",   () => _worldInteraction.PlaceWall(x, y));
            _ctx.Input.PlaceDoor           += (x, y) => _worldInteraction.SafeCall("PlaceDoor",   () => _worldInteraction.PlaceDoor(x, y));
            _ctx.Input.PlaceWire           += (x, y) => _worldInteraction.SafeCall("PlaceWire",   () => _worldInteraction.PlaceWire(x, y));
            _ctx.Input.PlaceGenerator      += (x, y) => _worldInteraction.SafeCall("PlaceGen",    () => _worldInteraction.PlaceGenerator(x, y));
            _ctx.Input.PlaceLamp           += (x, y) => _worldInteraction.SafeCall("PlaceLamp",   () => _worldInteraction.PlaceLamp(x, y));
            _ctx.Input.RemoveObject        += (x, y) => _worldInteraction.SafeCall("RemoveObject",() => _worldInteraction.RemoveObject(x, y));
            _ctx.Input.SwitchHand          += () => _worldInteraction.SafeCall("SwitchHand",      () => _ctx.State.ActiveHandIndex = 1 - _ctx.State.ActiveHandIndex);
            _ctx.Input.SelectSlot          += (i) => _worldInteraction.SafeCall("SelectSlot",     () => _ctx.State.ActiveSlotIndex = i);
            _ctx.Input.PlaceInPocket       += () => _worldInteraction.SafeCall("PlaceInPocket",   _worldInteraction.PlaceInPocket);
            _ctx.Input.UseItem             += () => _worldInteraction.SafeCall("UseItem",         _worldInteraction.UseItem);
            _ctx.Input.DropItem            += () => _worldInteraction.SafeCall("DropItem",        _worldInteraction.DropItem);
            _ctx.Input.ModifyOxygen        += (x, y, d) => _worldInteraction.SafeCall("ModifyO2",     () => _worldInteraction.ModifyOxygen(x, y, d));
            _ctx.Input.ModifyNitrogen      += (x, y, d) => _worldInteraction.SafeCall("ModifyN2",     () => _worldInteraction.ModifyNitrogen(x, y, d));
            _ctx.Input.ModifyCarbonDioxide += (x, y, d) => _worldInteraction.SafeCall("ModifyCO2",    () => _worldInteraction.ModifyCarbonDioxide(x, y, d));
            _ctx.Input.ModifyTemperature   += (x, y, d) => _worldInteraction.SafeCall("ModifyTemp",   () => _worldInteraction.ModifyTemperature(x, y, d));
            _ctx.Input.ModifyPlasma        += (x, y, d) => _worldInteraction.SafeCall("ModifyPlasma", () => _worldInteraction.ModifyPlasma(x, y, d));
        }

        private void OnRuntimeError(RuntimeErrorEntry entry)
        {
            var color = entry.Severity == RuntimeErrorSeverity.Critical
                ? new Color(1f, 0.4f, 0.4f, 1f)
                : new Color(1f, 0.85f, 0.5f, 1f);
            _ctx.Hud.Notify($"Ошибка [{entry.System}]: {entry.Message}", color);
        }

        private void LogOpenGLInfo()
        {
            try
            {
                Logger.Info("GL", $"Vendor:   {Gl.GetStringS(StringName.Vendor)}");
                Logger.Info("GL", $"Renderer: {Gl.GetStringS(StringName.Renderer)}");
                Logger.Info("GL", $"Version:  {Gl.GetStringS(StringName.Version)}");
                Logger.Info("GL", $"GLSL:     {Gl.GetStringS(StringName.ShadingLanguageVersion)}");

                int depthBits = Gl.GetInteger((GetPName)0x0D56);
                Logger.Info("GL", $"Default depth bits: {depthBits}");
            }
            catch (Exception ex)
            {
                Logger.Error("GL", "Failed to get OpenGL info", ex);
            }
        }

        private void UpdateWindowScale()
        {
            _windowScale = new Vector2(_ctx.ScreenWidth / 800f, _ctx.ScreenHeight / 600f);
        }

        protected override void OnResize(Vector2D<int> size)
        {
            base.OnResize(size);
            _ctx.ScreenWidth = size.X;
            _ctx.ScreenHeight = size.Y;
            Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            UpdateWindowScale();

            if (_ctx.Camera != null)
            {
                _ctx.Camera.ViewportWidth = size.X;
                _ctx.Camera.ViewportHeight = size.Y;
            }

            if (_ctx.Framebuffer != null && size.X > 0 && size.Y > 0)
                _ctx.Framebuffer.Resize(size.X, size.Y);
        }

        private void UpdateCamera3D()
        {
            const float TileSizeF = 32f;
            const float EyeHeight = 1.6f;

            _ctx.Camera.Position = new Vector3(
                _ctx.State.Player.Position.X / TileSizeF,
                EyeHeight,
                _ctx.State.Player.Position.Y / TileSizeF
            );
            _ctx.Camera.Yaw = _ctx.State.Player.Direction;
            _ctx.Camera.Pitch = _ctx.State.Player.Pitch;
        }

        protected override void OnUpdate(double deltaTime)
        {
            var (p, a, pw, h) = _ctx.State.Update((float)deltaTime);
            _ctx.PlayerMs = p;
            _ctx.AtmosMs = a;
            _ctx.PowerMs = pw;
            _ctx.HealthMs = h;

            UpdateCamera3D();

            _ctx.CurrentHit = Raycaster.Cast(
                _ctx.State.Map,
                _ctx.Camera.Position,
                _ctx.Camera.Forward,
                ReachDistance);

            _interactionMenu.Update((float)deltaTime);
            _ctx.Input.Update((float)deltaTime, _ctx.CurrentHit);
            _hotkeys.Update();

            _worldInteraction.SetDeltaTime((float)deltaTime);
            _worldInteraction.UpdateInput();

            Vector2 currentPos = _ctx.State.Player.Position;
            bool isMoving = Vector2.DistanceSquared(currentPos, _lastPlayerPos) > 0.01f;
            _lastPlayerPos = currentPos;

            _ctx.ViewModel.Update((float)deltaTime, isMoving);
            _ctx.Hud.Update((float)deltaTime);
        }

        protected override void OnRender(double deltaTime)
        {
            _ctx.Fps = _fps;
            _pipeline.Render();

            _ctx.State.Runtime.NextFrame();
            _frameCount++;
            if (_fpsTimer.Elapsed.TotalSeconds >= 1.0)
            {
                _fps = _frameCount / (float)_fpsTimer.Elapsed.TotalSeconds;
                _frameCount = 0;
                _fpsTimer.Restart();
                _ctx.State.Runtime.SetFps(_fps);

                Logger.Info("Perf",
                    $"FPS={_fps:F1} Player={_ctx.PlayerMs:F2}ms Atmos={_ctx.AtmosMs:F2}ms " +
                    $"Render={_ctx.RenderMs:F2}ms Chunks={_ctx.World.LastDrawnTiles} " +
                    $"Uptime={_ctx.State.Runtime.FormatRealTime()} Errors={_ctx.State.Errors.TotalCount}");
            }
        }
    }
}