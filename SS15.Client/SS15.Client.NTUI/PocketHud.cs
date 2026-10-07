using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.NTUI
{
    public class PocketHud
    {
        private Texture2D _pocketTex;
        private Texture2D _itemTex;

        private const int PocketSize = 32;
        private const int PocketSpacing = 5;
        private const int PocketCount = 4;

        public PocketHud(Texture2D pocketTex, Texture2D itemTex)
        {
            _pocketTex = pocketTex;
            _itemTex = itemTex;
        }

        public void Draw(SpriteBatch spriteBatch, Inventory inventory, Vector2 startPosition)
        {
            for (int i = 0; i < PocketCount; i++)
            {
                Vector2 pos = startPosition + new Vector2(i * (PocketSize + PocketSpacing), 0);
                spriteBatch.Draw(_pocketTex, pos, null, Vector2.One, 0, Vector2.Zero, Color.White);

                int slotIndex = i + 2; // карманы начинаются с индекса 2
                var item = inventory.GetItem(slotIndex);
                if (item != null)
                {
                    Vector2 center = pos + new Vector2(PocketSize / 2f, PocketSize / 2f);
                    DrawItem(spriteBatch, item, center);
                }
            }
        }

        private void DrawItem(SpriteBatch spriteBatch, Item item, Vector2 center)
        {
            float size = 16f;
            spriteBatch.Draw(_itemTex, center - new Vector2(size / 2f, size / 2f), null,
                new Vector2(size, size), 0, Vector2.Zero,
                new Color(item.Color[0] / 255f, item.Color[1] / 255f, item.Color[2] / 255f, 1f));
        }
    }
}