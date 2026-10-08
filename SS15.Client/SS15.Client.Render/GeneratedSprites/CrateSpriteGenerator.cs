#nullable disable
using System;
using System.IO;
using Engine.Rendering;
using Silk.NET.OpenGL;
using SS15.Client.Core;

namespace SS15.Client.Render.GeneratedSprites
{
    public static class CrateSpriteGenerator
    {
        public static Texture2D LoadOrCreate(GL gl)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Machinery", "crate.png");
            if (File.Exists(path))
            {
                try { return new Texture2D(gl, path); }
                catch { }
            }

            Logger.Info("CrateSprite", "Using procedural crate texture");
            return Create(gl, 32);
        }

        public static Texture2D Create(GL gl, int size)
        {
            byte[] data = new byte[size * size * 4];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = (y * size + x) * 4;

                    byte r = 140, g = 90, b = 45;

                    bool border = x < 2 || x >= size - 2 || y < 2 || y >= size - 2;
                    bool stripe = (x == size / 2) || (y == size / 2);
                    bool corner = (x < 4 && y < 4) || (x >= size - 4 && y < 4)
                               || (x < 4 && y >= size - 4) || (x >= size - 4 && y >= size - 4);

                    if (border)      { r = 90;  g = 55;  b = 25; }
                    else if (stripe) { r = 110; g = 70;  b = 35; }
                    if (corner)      { r = 170; g = 170; b = 170; }

                    data[i]     = r;
                    data[i + 1] = g;
                    data[i + 2] = b;
                    data[i + 3] = 255;
                }
            }

            return new Texture2D(gl, data, size, size);
        }
    }
}