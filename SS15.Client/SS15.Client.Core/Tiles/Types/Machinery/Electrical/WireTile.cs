#nullable disable
namespace SS15.Client.Core.Tiles
{
    public sealed class WireTile : Tile
    {
        public override TileType Type => TileType.Wire;
        public override bool IsConductive => true;
        public override ChunkSlot GetRenderSlot(GameMap map, int x, int z) => ChunkSlot.Floor;
    }
}