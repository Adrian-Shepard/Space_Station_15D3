using System;
using System.Drawing;
using System.IO;
using Engine.Rendering;
using Silk.NET.OpenGL;

namespace SS15.Client.Render.GeneratedSprites
{
    public static class DoorSpriteGenerator
    {
        public static Texture2D LoadOrCreateOpen(GL gl, string relativePath = "NTSouse/door_open.png")
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
                        // Открытая дверь: рамка по краям, проём в центре
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 200, 200, 200); // светло-серая рамка
                        if (x >= 4 && x <= 27 && y >= 4 && y <= 27)
                            c = System.Drawing.Color.FromArgb(0, 0, 0, 0); // прозрачный проём
                        // Боковые стойки
                        if (x < 4 || x > 27 || y < 4 || y > 27)
                            c = System.Drawing.Color.FromArgb(255, 150, 150, 150);
                        bmp.SetPixel(x, y, c);
                    }
                }
                SpriteGenerator.SaveTextureFromBitmap(relativePath, bmp);
            }

            return new Texture2D(gl, fullPath);
        }

        public static Texture2D LoadOrCreateClosed(GL gl, string relativePath = "NTSouse/door_closed.png")
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
                        // Закрытая дверь: сплошная серая панель
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 120, 120, 120);
                        // Ручка
                        if (x >= 24 && x <= 26 && y >= 14 && y <= 17)
                            c = System.Drawing.Color.FromArgb(255, 255, 255, 0); // жёлтая ручка
                        // Щель
                        if (x == 16 && y >= 4 && y <= 27)
                            c = System.Drawing.Color.FromArgb(255, 30, 30, 30);
                        bmp.SetPixel(x, y, c);
                    }
                }
                SpriteGenerator.SaveTextureFromBitmap(relativePath, bmp);
            }

            return new Texture2D(gl, fullPath);
        }
    }
}