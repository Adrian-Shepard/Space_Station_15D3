#nullable disable
using Engine.Rendering;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SS15.Client.Commands;
using SS15.Client.Input;
using SS15.Client.NTUI;
using SS15.Client.Render;

using Framebuffer = SS15.Client.Render.Framebuffer;
using RaycastHit = SS15.Client.Render.RaycastHit;

namespace SS15.Client
{
    public class GameContext
    {
        public GL Gl;
        public IWindow Window;
        public IKeyboard Keyboard;
        public IMouse Mouse;

        public float ScreenWidth;
        public float ScreenHeight;

        public GameState State;
        public Camera3D Camera;

        public Framebuffer Framebuffer;
        public SpriteBatch SpriteBatch;
        public AssetLibrary Assets;

        public WorldRenderer3D World;
        public ViewModelRenderer ViewModel;
        public HudRenderer Hud;

        public InputHandler Input;
        public InteractionMenu Menu;
        public RaycastHit CurrentHit;

        public CommandDispatcher Commands;

        public float Fps;
        public float PlayerMs;
        public float AtmosMs;
        public float PowerMs;
        public float HealthMs;
        public float RenderMs;
    }
}