#nullable disable
using Silk.NET.OpenGL;
using System;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public class Framebuffer : IDisposable
    {
        private GL _gl;
        public uint Fbo { get; private set; }
        public uint ColorTex { get; private set; }
        public uint DepthRbo { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Framebuffer(GL gl, int width, int height)
        {
            _gl = gl;
            Width = width;
            Height = height;
            Create();
        }

        private unsafe void Create()
        {
            Fbo = _gl.GenFramebuffer();
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

            // Color attachment
            ColorTex = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, ColorTex);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                (uint)Width, (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, ColorTex, 0);

            // Depth attachment (renderbuffer)
            DepthRbo = _gl.GenRenderbuffer();
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRbo);
            _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24,
                (uint)Width, (uint)Height);
            _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer, DepthRbo);

            var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != GLEnum.FramebufferComplete)
            {
                Logger.Error("Framebuffer", $"FBO incomplete: {status}");
                throw new Exception($"Framebuffer incomplete: {status}");
            }

            Logger.Info("Framebuffer", $"Created FBO={Fbo} Color={ColorTex} Depth={DepthRbo} {Width}x{Height}");

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            _gl.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind()
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
            _gl.Viewport(0, 0, (uint)Width, (uint)Height);
        }

        public void Unbind()
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Resize(int width, int height)
        {
            if (width == Width && height == Height) return;
            if (width <= 0 || height <= 0) return;

            Width = width;
            Height = height;

            // Пересоздание
            _gl.DeleteFramebuffer(Fbo);
            _gl.DeleteTexture(ColorTex);
            _gl.DeleteRenderbuffer(DepthRbo);
            Create();
        }

        public void BlitToScreen(int screenWidth, int screenHeight)
        {
            _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Fbo);
            _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            _gl.BlitFramebuffer(
                0, 0, Width, Height,
                0, 0, screenWidth, screenHeight,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear);
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
            if (Fbo != 0) _gl.DeleteFramebuffer(Fbo);
            if (ColorTex != 0) _gl.DeleteTexture(ColorTex);
            if (DepthRbo != 0) _gl.DeleteRenderbuffer(DepthRbo);
            Fbo = 0;
            ColorTex = 0;
            DepthRbo = 0;
        }
    }
}