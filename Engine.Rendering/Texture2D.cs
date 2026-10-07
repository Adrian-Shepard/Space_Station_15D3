//Texture2D.cs
#nullable disable
using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.IO;

namespace Engine.Rendering
{
    public class Texture2D : IDisposable
    {
        public uint Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private GL _gl;

        public Texture2D(GL gl, string path)
        {
            _gl = gl;
            LoadFromFile(path);
        }

        public Texture2D(GL gl, byte[] pixelData, int width, int height)
        {
            _gl = gl;
            Width = width;
            Height = height;
            CreateTexture(pixelData);
        }

        private unsafe void LoadFromFile(string path)
        {
            using var stream = File.OpenRead(path);
            ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            Width = result.Width;
            Height = result.Height;
            CreateTexture(result.Data);
        }

        private unsafe void CreateTexture(byte[] data)
        {
            Handle = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, Handle);

            fixed (byte* ptr = data)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                    (uint)Width, (uint)Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureUnit);
            _gl.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void Dispose()
        {
            if (Handle != 0) _gl.DeleteTexture(Handle);
            Handle = 0;
        }
    }
}