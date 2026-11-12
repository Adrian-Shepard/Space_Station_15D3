using System;
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Commands
{
    public sealed class DropItemCommand : GameCommand
    {
        public bool Throw;

        public override CommandType Type => CommandType.DropItem;

        public override GameCommandResult Apply(GameContext ctx)
        {
            int h = ctx.State.ActiveHandIndex;
            var item = ctx.State.Player.Inventory.RemoveItem(h);
            if (item == null) return GameCommandResult.Reject("Рука пуста");

            var p = ctx.State.Player;

            if (Throw)
            {
                float dirX = MathF.Cos(p.Direction);
                float dirY = MathF.Sin(p.Direction);
                var dir = new Vector2(dirX, dirY);
                var from = p.Position + dir * 10f;
                float sliding = SlidingFrictionFor(item.Type);
                ctx.State.ItemFlight.Throw(item, from, dir, p.Pitch, 700f, sliding);
                return GameCommandResult.Ok($"Брошено: {item.Name}");
            }

            int tx = (int)(p.Position.X / ctx.State.Map.TileSize);
            int ty = (int)(p.Position.Y / ctx.State.Map.TileSize);
            ctx.State.Map.AddItemToTile(tx, ty, item);
            return GameCommandResult.Ok($"Выброшено: {item.Name}");
        }

        private static float SlidingFrictionFor(ItemType type) => type switch
        {
            ItemType.Medkit => 4.0f,
            ItemType.Food   => 3.0f,
            ItemType.Tool   => 2.0f,
            _               => 3.5f,
        };
    }
}