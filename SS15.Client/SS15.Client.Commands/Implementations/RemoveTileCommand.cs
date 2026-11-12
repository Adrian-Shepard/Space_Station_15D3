#nullable disable
using SS15.Client.Core;

namespace SS15.Client.Commands
{
    public sealed class RemoveTileCommand : GameCommand
    {
        public int X;
        public int Z;

        public override CommandType Type => CommandType.RemoveTile;

        public override GameCommandResult Apply(GameContext ctx)
        {
            var map = ctx.State.Map;
            var tile = map.GetTileType(X, Z);

            if (tile == TileType.Floor)
            {
                map.SetTile(X, Z, TileType.Space);
                ctx.State.Atmosphere.SetTileToSpace(X, Z);
                return GameCommandResult.Ok("Убрано в космос");
            }

            if (tile == TileType.Space)
            {
                map.SetTile(X, Z, TileType.Floor);
                ctx.State.Atmosphere.SetTileToFloor(X, Z);
                return GameCommandResult.Ok("Восстановлен пол");
            }

            map.SetTile(X, Z, TileType.Floor);
            ctx.State.Atmosphere.SetTileToFloor(X, Z);
            return GameCommandResult.Ok("Удалено");
        }
    }
}