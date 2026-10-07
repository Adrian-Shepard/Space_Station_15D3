using System;
using System.Drawing;
using System.IO;
using Engine.Rendering;
using Silk.NET.OpenGL;

namespace SS15.Client.Render.GeneratedSprites
{
    public static class GeneratorSpriteGenerator
    {
        public static Texture2D LoadOrCreate(GL gl, string relativePath = "NTSouse/generator.png")
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
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 0, 0, 200);
                        if (y >= 2 && y <= 6 && x >= 4 && x <= 27)
                            c = System.Drawing.Color.FromArgb(255, 50, 50, 255);
                        if (x >= 12 && x <= 19 && y >= 8 && y <= 10)
                            c = System.Drawing.Color.FromArgb(255, 0, 255, 0);
                        bmp.SetPixel(x, y, c);
                    }
                }
                SpriteGenerator.SaveTextureFromBitmap(relativePath, bmp);
            }

            return new Texture2D(gl, fullPath);
        }
    }
}