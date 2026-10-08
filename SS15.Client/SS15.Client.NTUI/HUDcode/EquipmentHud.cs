using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.NTUI
{
    public class EquipmentHud
    {
        private Texture2D _beltTex;
        private Texture2D _backTex;
        private Texture2D _itemTex;

        private const int SlotSize = 32;
        private const int Spacing = 5;

        public EquipmentHud(Texture2D beltTex, Texture2D backTex, Texture2D itemTex)
        {
            _beltTex = beltTex;
            _backTex = backTex;
            _itemTex = itemTex;
        }

        public void Draw(SpriteBatch spriteBatch, Inventory inventory, Vector2 startPosition)
        {
            // Пояс (слот 6)
            spriteBatch.Draw(_beltTex, startPosition, null, Vector2.One, 0, Vector2.Zero, Color.White);
            var beltItem = inventory.GetItem(6);
            if (beltItem != null)
                DrawItem(spriteBatch, beltItem, startPosition + new Vector2(SlotSize / 2f, SlotSize / 2f));

            // Спина (слот 7)
            Vector2 backPos = startPosition + new Vector2(SlotSize + Spacing, 0);
            spriteBatch.Draw(_backTex, backPos, null, Vector2.One, 0, Vector2.Zero, Color.White);
            var backItem = inventory.GetItem(7);
            if (backItem != null)
                DrawItem(spriteBatch, backItem, backPos + new Vector2(SlotSize / 2f, SlotSize / 2f));
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