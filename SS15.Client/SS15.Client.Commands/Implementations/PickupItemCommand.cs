namespace SS15.Client.Commands
{
    public sealed class PickupItemCommand : GameCommand
    {
        public int X;
        public int Z;

        public override CommandType Type => CommandType.PickupItem;

        public override GameCommandResult Apply(GameContext ctx)
        {
            var items = ctx.State.Map.GetItemsAtTile(X, Z);
            if (items == null || items.Count == 0)
                return GameCommandResult.Reject("Нет предметов");

            var item = items[0];
            ctx.State.Map.RemoveItemAtTile(X, Z, item);

            if (!ctx.State.Player.Inventory.AddToHand(item) &&
                !ctx.State.Player.Inventory.AddToPocket(item) &&
                !ctx.State.Player.Inventory.AddToBelt(item) &&
                !ctx.State.Player.Inventory.AddToBack(item))
            {
                ctx.State.Map.AddItemToTile(X, Z, item);
                return GameCommandResult.Reject("Инвентарь полон");
            }

            return GameCommandResult.Ok($"Подобрано: {item.Name}");
        }
    }
}