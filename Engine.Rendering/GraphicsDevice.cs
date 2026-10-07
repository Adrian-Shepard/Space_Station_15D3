#nullable disable
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;

namespace Engine.Rendering
{
    public class GraphicsDevice : IDisposable
    {
        public IWindow Window { get; private set; }
        public GL Gl { get; private set; }

        public GraphicsDevice(int width, int height, string title)
        {
            var options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(width, height),
                Title = title,

                // КРИТИЧНО для 3D: создать depth buffer
                PreferredDepthBufferBits = 24,
                PreferredStencilBufferBits = 8,

                // MSAA (опционально, попробуйте 0 или 4)
                Samples = 0,

                API = new GraphicsAPI(
                    ContextAPI.OpenGL,
                    ContextProfile.Core,
                    ContextFlags.ForwardCompatible,
                    new APIVersion(4, 6))
            };

            Window = Silk.NET.Windowing.Window.Create(options);
            Window.Load += OnLoadInternal;
            Window.Update += OnUpdateInternal;
            Window.Render += OnRenderInternal;
            Window.Resize += OnResizeInternal;
            Window.Closing += OnClosingInternal;
        }

        protected virtual void OnLoad() { }
        protected virtual void OnUpdate(double deltaTime) { }
        protected virtual void OnRender(double deltaTime) { }
        protected virtual void OnResize(Vector2D<int> size) { }
        protected virtual void OnClosing() { }

        private void OnLoadInternal()
        {
            Gl = Window.CreateOpenGL();
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            OnLoad();
        }

        private void OnUpdateInternal(double deltaTime) => OnUpdate(deltaTime);
        private void OnRenderInternal(double deltaTime) => OnRender(deltaTime);
        private void OnResizeInternal(Vector2D<int> size) => OnResize(size);
        private void OnClosingInternal() => OnClosing();

        public void Run() => Window.Run();

        public void Dispose()
        {
            Gl?.Dispose();
            Window?.Dispose();
        }
    }
}