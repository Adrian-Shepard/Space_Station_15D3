#nullable disable
using SS15.Client.Core;

namespace SS15.Client.Commands
{
    public sealed class PlaceTileCommand : GameCommand
    {
        public int X;
        public int Z;
        public TileType NewTileType;

        public override CommandType Type => CommandType.PlaceTile;

        public override GameCommandResult Apply(GameContext ctx)
        {
            var map = ctx.State.Map;
            var existing = map.GetTileType(X, Z);

            if (existing == TileType.Wall || existing == TileType.ReinforcedWall || existing == TileType.Door)
                return GameCommandResult.Reject("Место занято");

            map.SetTile(X, Z, NewTileType);

            if (NewTileType == TileType.Wall || NewTileType == TileType.ReinforcedWall)
                ctx.State.Atmosphere.SetWall(X, Z, true);

            return GameCommandResult.Ok();
        }
    }
}