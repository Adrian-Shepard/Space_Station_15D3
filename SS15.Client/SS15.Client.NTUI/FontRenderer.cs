using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine.Rendering;
using Silk.NET.OpenGL;

using DrawingColor = System.Drawing.Color;
using RenderingColor = Engine.Rendering.Color;
using DrawingRectangle = System.Drawing.Rectangle;
using RenderingRectangle = Engine.Rendering.Rectangle;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SS15.Client.NTUI
{
    public class FontRenderer
    {
        private Texture2D _fontTexture;
        private Dictionary<char, (DrawingRectangle rect, float offsetX, float offsetY)> _glyphs;
        private int _cellWidth;
        private int _cellHeight;
        private float _spacingFactor = 0.7f;

        public FontRenderer(GL gl, string fontFamily = "Arial", float fontSize = 16, FontStyle fontStyle = FontStyle.Bold)
        {
            _cellWidth = 32;
            _cellHeight = 32;
            _glyphs = new Dictionary<char, (DrawingRectangle, float, float)>();
            _fontTexture = GenerateFontTexture(gl, fontFamily, fontSize, fontStyle);
        }

        /// Набор символов: ASCII + Ё/ё + кириллица + EM DASH + DEGREE.
        private static string BuildCharset()
        {
            var sb = new System.Text.StringBuilder(200);

            // ASCII 32..126 (пробел, цифры, латиница, знаки препинания)
            for (char c = (char)32; c <= 126; c++) sb.Append(c);

            // Ё и ё
            sb.Append('Ё');
            sb.Append('ё');

            // Кириллица А-Я, а-я
            for (char c = (char)0x0410; c <= (char)0x044F; c++) sb.Append(c);

            // Тире EM DASH
            sb.Append('—');

            // Знак градуса
            sb.Append('°');

            return sb.ToString();
        }

        private Texture2D GenerateFontTexture(GL gl, string fontFamily, float fontSize, FontStyle fontStyle)
        {
            string chars = BuildCharset();

            int columns = 20;
            int rows = (int)Math.Ceiling(chars.Length / (float)columns);
            int textureWidth = columns * _cellWidth;
            int textureHeight = rows * _cellHeight;

            using (Bitmap bitmap = new Bitmap(textureWidth, textureHeight))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                using (Font font = new Font(fontFamily, fontSize, fontStyle))
                {
                    g.Clear(DrawingColor.Transparent);
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    int index = 0;
                    foreach (char c in chars)
                    {
                        int col = index % columns;
                        int row = index / columns;
                        float x = col * _cellWidth;
                        float y = row * _cellHeight;

                        g.DrawString(c.ToString(), font, Brushes.White, x, y);

                        var rect = new DrawingRectangle((int)x, (int)y, _cellWidth, _cellHeight);
                        _glyphs[c] = (rect, 0, 0);

                        index++;
                    }
                }

                int width = bitmap.Width;
                int height = bitmap.Height;
                byte[] data = new byte[width * height * 4];

                var bmpData = bitmap.LockBits(
                    new DrawingRectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    DrawingPixelFormat.Format32bppArgb);

                try
                {
                    IntPtr srcPtr = bmpData.Scan0;
                    int srcStride = bmpData.Stride;

                    for (int py = 0; py < height; py++)
                    {
                        IntPtr rowPtr = IntPtr.Add(srcPtr, py * srcStride);
                        int dstRow = py * width * 4;
                        Marshal.Copy(rowPtr, data, dstRow, width * 4);

                        // BGRA → RGBA
                        for (int px = 0; px < width; px++)
                        {
                            int i = dstRow + px * 4;
                            byte b = data[i];
                            byte g2 = data[i + 1];
                            byte r = data[i + 2];
                            byte a = data[i + 3];
                            data[i]     = r;
                            data[i + 1] = g2;
                            data[i + 2] = b;
                            data[i + 3] = a;
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                return new Texture2D(gl, data, width, height);
            }
        }

        public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, float scale, RenderingColor color)
        {
            float x = position.X;
            foreach (char c in text)
            {
                if (_glyphs.TryGetValue(c, out var glyph))
                {
                    var (rect, offsetX, offsetY) = glyph;
                    var sourceRect = new RenderingRectangle(
                        (int)rect.X,
                        (int)rect.Y,
                        (int)rect.Width,
                        (int)rect.Height
                    );
                    spriteBatch.Draw(_fontTexture,
                        new Vector2(x + offsetX * scale, position.Y + offsetY * scale),
                        sourceRect,
                        new Vector2(scale, scale),
                        0, Vector2.Zero, color);
                    x += rect.Width * scale * _spacingFactor;
                }
                else
                {
                    // Символа нет — сдвигаем курсор, чтобы слова не слипались
                    x += _cellWidth * scale * _spacingFactor;
                }
            }
        }
    }
}