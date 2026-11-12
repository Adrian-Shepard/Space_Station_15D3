#nullable disable
namespace SS15.Client.Core.Tiles
{
    public sealed class FloorTile : Tile
    {
        public override TileType Type => TileType.Floor;
        public override ChunkSlot GetRenderSlot(GameMap map, int x, int z) => ChunkSlot.Floor;
    }
}