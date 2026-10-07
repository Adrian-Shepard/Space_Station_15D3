using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.NTUI
{
    public class HandsHud
    {
        private Texture2D _armsPreviewTex;
        private Texture2D _itemTex;
        private Texture2D _activeHandTex;

        private const int PanelWidth = 66;
        private const int PanelHeight = 45;

        private static readonly Vector2 LeftHandPos = new Vector2(1, 10);
        private static readonly Vector2 RightHandPos = new Vector2(33, 10);
        private const int HandSlotSize = 32;

        public HandsHud(Texture2D armsPreview, Texture2D itemTex, Texture2D activeHandTex)
        {
            _armsPreviewTex = armsPreview;
            _itemTex = itemTex;
            _activeHandTex = activeHandTex;
        }

        public void Draw(SpriteBatch spriteBatch, Inventory inventory, int activeHandIndex, Vector2 panelPosition)
        {
            spriteBatch.Draw(_armsPreviewTex, panelPosition, null, Vector2.One, 0, Vector2.Zero, Color.White);

            Color highlight = new Color(1f, 1f, 0f, 0.3f);
            Vector2 highlightPos = activeHandIndex == 0 ? LeftHandPos : RightHandPos;
            spriteBatch.Draw(_activeHandTex, panelPosition + highlightPos, null,
                new Vector2(HandSlotSize, HandSlotSize), 0, Vector2.Zero, highlight);

            var leftItem = inventory.GetItem(0);
            if (leftItem != null)
                DrawItem(spriteBatch, leftItem, panelPosition + LeftHandPos + new Vector2(HandSlotSize / 2f, HandSlotSize / 2f));

            var rightItem = inventory.GetItem(1);
            if (rightItem != null)
                DrawItem(spriteBatch, rightItem, panelPosition + RightHandPos + new Vector2(HandSlotSize / 2f, HandSlotSize / 2f));
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