using System.Numerics;

namespace SS15.Client.Commands
{
    public sealed class ToggleDoorCommand : GameCommand
    {
        public int X;
        public int Z;

        public override CommandType Type => CommandType.ToggleDoor;

        public override GameCommandResult Apply(GameContext ctx)
        {
            int gid = ctx.State.Map.GetDoorGroupId(X, Z);
            if (gid <= 0) return GameCommandResult.Reject("Не дверь");

            bool open = ctx.State.Map.IsDoorOpen(X, Z);
            bool newState = !open;

            if (!newState && AnyPlayerInDoorGroup(ctx, gid))
                return GameCommandResult.Reject("Дверь заблокирована — кто-то в проёме");

            ctx.State.Map.SetDoorOpen(X, Z, newState);
            var tiles = ctx.State.Map.GetDoorTilesInGroup(gid);
            foreach (var (dx, dy) in tiles)
                ctx.State.Atmosphere.SetDoorState(dx, dy, newState);

            return GameCommandResult.Ok(newState ? "Дверь открыта" : "Дверь закрыта");
        }

        private static bool AnyPlayerInDoorGroup(GameContext ctx, int gid)
        {
            var tiles = ctx.State.Map.GetDoorTilesInGroup(gid);
            var p = ctx.State.Player;
            int ts = ctx.State.Map.TileSize;
            float half = p.PlayerHalf;

            foreach (var (tx, ty) in tiles)
            {
                float tx0 = tx * ts;
                float ty0 = ty * ts;
                if (p.Position.X + half > tx0 && p.Position.X - half < tx0 + ts &&
                    p.Position.Y + half > ty0 && p.Position.Y - half < ty0 + ts)
                    return true;
            }
            return false;
        }
    }
}