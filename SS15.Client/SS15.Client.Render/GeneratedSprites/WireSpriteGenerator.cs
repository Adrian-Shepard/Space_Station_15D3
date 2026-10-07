using System;
using System.Drawing;
using System.IO;
using Engine.Rendering;
using Silk.NET.OpenGL;

namespace SS15.Client.Render.GeneratedSprites
{
    public static class WireSpriteGenerator
    {
        public static Texture2D LoadOrCreate(GL gl, string relativePath = "NTSouse/wire.png")
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, "Assets", relativePath);
            if (File.Exists(fullPath))
            {
                return new Texture2D(gl, fullPath);
            }

            int size = 32;
            using (Bitmap bmp = new Bitmap(size, size))
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 255, 200, 0);
                        if ((x + y) % 5 == 0) c = System.Drawing.Color.FromArgb(255, 180, 140, 0);
                        bmp.SetPixel(x, y, c);
                    }
                }
                SpriteGenerator.SaveTextureFromBitmap(relativePath, bmp);
            }

            return new Texture2D(gl, fullPath);
        }
    }
}