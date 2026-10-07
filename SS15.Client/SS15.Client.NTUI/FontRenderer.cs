using System;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering;
using Silk.NET.OpenGL;

using DrawingColor = System.Drawing.Color;
using RenderingColor = Engine.Rendering.Color;
using DrawingRectangle = System.Drawing.Rectangle;
using RenderingRectangle = Engine.Rendering.Rectangle;

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

        private Texture2D GenerateFontTexture(GL gl, string fontFamily, float fontSize, FontStyle fontStyle)
        {
            string chars = "";
            for (char c = (char)32; c <= 126; c++) chars += c;

            int columns = 16;
            int rows = (int)Math.Ceiling(chars.Length / (float)columns);
            int textureWidth = columns * _cellWidth;
            int textureHeight = rows * _cellHeight;

            using (Bitmap bitmap = new Bitmap(textureWidth, textureHeight))
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

                int width = bitmap.Width;
                int height = bitmap.Height;
                byte[] data = new byte[width * height * 4];

                for (int py = 0; py < height; py++)
                {
                    for (int px = 0; px < width; px++)
                    {
                        DrawingColor pixel = bitmap.GetPixel(px, py);
                        int offset = (py * width + px) * 4;
                        data[offset] = pixel.R;
                        data[offset + 1] = pixel.G;
                        data[offset + 2] = pixel.B;
                        data[offset + 3] = pixel.A;
                    }
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
            }
        }
    }
}