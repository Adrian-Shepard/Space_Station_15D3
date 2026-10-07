using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.NTUI
{
    public class StatusBars
    {
        private Texture2D _backgroundTexture;
        private Texture2D _fillTexture;

        public StatusBars(Texture2D background, Texture2D fill)
        {
            _backgroundTexture = background;
            _fillTexture = fill;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float percent, float width, float height, Color fillColor)
        {
            spriteBatch.Draw(_backgroundTexture, position, null, new Vector2(width, height), 0, Vector2.Zero, Color.White);
            spriteBatch.Draw(_fillTexture, position, null, new Vector2(width * percent, height), 0, Vector2.Zero, fillColor);
        }
    }
}