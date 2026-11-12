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

        public Texture2D(GL gl, string path, bool repeat = false)
        {
            _gl = gl;
            LoadFromFile(path, repeat);
        }

        public Texture2D(GL gl, byte[] pixelData, int width, int height, bool repeat = false)
        {
            _gl = gl;
            Width = width;
            Height = height;
            CreateTexture(pixelData, repeat);
        }

        private unsafe void LoadFromFile(string path, bool repeat)
        {
            using var stream = File.OpenRead(path);
            ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            Width = result.Width;
            Height = result.Height;
            CreateTexture(result.Data, repeat);
        }

        private unsafe void CreateTexture(byte[] data, bool repeat)
        {
            Handle = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, Handle);

            fixed (byte* ptr = data)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                    (uint)Width, (uint)Height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            // Мипмапы — для дальних объектов
            _gl.GenerateMipmap(TextureTarget.Texture2D);

            // Nearest для мелких пиксель-арт текстур, Linear для крупных
            bool pixelArt = Width <= 64 && Height <= 64;

            if (pixelArt)
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.NearestMipmapNearest);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Nearest);
            }
            else
            {
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
            }

            var wrap = repeat ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge;
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);

            _gl.BindTexture(TextureTarget.Texture2D, 0);
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