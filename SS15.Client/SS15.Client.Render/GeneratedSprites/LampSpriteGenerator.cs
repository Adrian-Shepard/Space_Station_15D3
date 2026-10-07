using System;
using System.Drawing;
using System.IO;
using Engine.Rendering;
using Silk.NET.OpenGL;

namespace SS15.Client.Render.GeneratedSprites
{
    public static class LampSpriteGenerator
    {
        public static Texture2D LoadOrCreate(GL gl, string relativePath = "NTSouse/lamp_off.png")
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
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 100, 100, 100);
                        if (x >= 6 && x <= 25 && y >= 6 && y <= 25)
                            c = System.Drawing.Color.FromArgb(255, 180, 180, 180);
                        if (x >= 10 && x <= 21 && y >= 10 && y <= 21)
                            c = System.Drawing.Color.FromArgb(255, 220, 220, 200);
                        bmp.SetPixel(x, y, c);
                    }
                }
                SpriteGenerator.SaveTextureFromBitmap(relativePath, bmp);
            }

            return new Texture2D(gl, fullPath);
        }

        public static Texture2D LoadOrCreateOn(GL gl, string relativePath = "NTSouse/lamp_on.png")
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
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 255, 255, 150);
                        if (x >= 10 && x <= 21 && y >= 10 && y <= 21)
                            c = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                        bmp.SetPixel(x, y, c);
                    }
                }
                SpriteGenerator.SaveTextureFromBitmap(relativePath, bmp);
            }

            return new Texture2D(gl, fullPath);
        }
    }
}