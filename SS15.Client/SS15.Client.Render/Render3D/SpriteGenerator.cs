using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Engine.Rendering;
using Silk.NET.OpenGL;

namespace SS15.Client.Render
{
    public static class SpriteGenerator
    {
        public static Texture2D LoadTexture(GL gl, string relativePath, Func<GL, Texture2D> fallbackGenerator = null)
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, "Assets", relativePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    return new Texture2D(gl, fullPath);
                }
                catch { }
            }
            return fallbackGenerator != null ? fallbackGenerator(gl) : CreatePlaceholderTexture(gl);
        }

        public static Texture2D CreateSolidTexture(GL gl, byte[] color)
        {
            return new Texture2D(gl, color, 1, 1);
        }

        public static Texture2D CreatePlaceholderTexture(GL gl, int tileSize = 32)
        {
            int size = tileSize;
            byte[] data = new byte[size * size * 4];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = (y * size + x) * 4;
                    bool isOrange = ((x / 8) + (y / 8)) % 2 == 0;
                    data[index] = isOrange ? (byte)255 : (byte)0;
                    data[index + 1] = isOrange ? (byte)140 : (byte)0;
                    data[index + 2] = 0;
                    data[index + 3] = 255;
                }
            }
            return new Texture2D(gl, data, size, size);
        }

        public static Texture2D CreateFloorTexture(GL gl, int tileSize = 32)
        {
            int size = tileSize;
            byte[] data = new byte[size * size * 4];
            Random rng = new Random(12345);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = (y * size + x) * 4;
                    byte r = 190, g = 190, b = 190;
                    int noise = rng.Next(-15, 16);
                    r = (byte)Math.Clamp(r + noise, 0, 255);
                    g = (byte)Math.Clamp(g + noise, 0, 255);
                    b = (byte)Math.Clamp(b + noise, 0, 255);
                    data[index] = r; data[index + 1] = g; data[index + 2] = b; data[index + 3] = 255;
                }
            }
            return new Texture2D(gl, data, size, size);
        }

        public static Texture2D CreateWallTexture(GL gl, int tileSize = 32)
        {
            int size = tileSize;
            byte[] data = new byte[size * size * 4];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = (y * size + x) * 4;
                    byte r = 60, g = 60, b = 60;
                    if (y % 8 == 0) { r = 30; g = 30; b = 30; }
                    else if ((y / 8) % 2 == 0 && x % 16 == 0) { r = 30; g = 30; b = 30; }
                    else if ((y / 8) % 2 == 1 && (x + 8) % 16 == 0) { r = 30; g = 30; b = 30; }
                    data[index] = r; data[index + 1] = g; data[index + 2] = b; data[index + 3] = 255;
                }
            }
            return new Texture2D(gl, data, size, size);
        }

        public static Texture2D CreatePlayerTexture(GL gl, int tileSize = 32)
        {
            int size = tileSize;
            byte[] data = new byte[size * size * 4];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = (y * size + x) * 4;
                    byte r = 255, g = 0, b = 0, a = 255;
                    float dx = x - size / 2f + 0.5f;
                    float dy = y - size / 2f + 0.5f;
                    if (MathF.Sqrt(dx * dx + dy * dy) > size / 2f)
                        a = 0;
                    else
                    {
                        if ((y >= 10 && y <= 14) && (x >= 8 && x <= 12) ||
                            (y >= 10 && y <= 14) && (x >= 20 && x <= 24))
                        { r = 255; g = 255; b = 255; }
                        else if (y == 20 && x >= 12 && x <= 20)
                        { r = 255; g = 255; b = 255; }
                    }
                    data[index] = r; data[index + 1] = g; data[index + 2] = b; data[index + 3] = a;
                }
            }
            return new Texture2D(gl, data, size, size);
        }

        public static Texture2D CreateSpaceTexture(GL gl, int tileSize = 32)
        {
            int size = tileSize;
            byte[] data = new byte[size * size * 4];
            Random rng = new Random(54321);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = (y * size + x) * 4;
                    data[index] = 0; data[index + 1] = 0; data[index + 2] = 0; data[index + 3] = 255;
                    if (rng.Next(100) < 3)
                    {
                        data[index] = 255; data[index + 1] = 255; data[index + 2] = 255;
                    }
                }
            }
            return new Texture2D(gl, data, size, size);
        }

        public static void SaveTextureFromBitmap(string relativePath, Bitmap bitmap)
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, "Assets", relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            bitmap.Save(fullPath, ImageFormat.Png);
        }
    }
}