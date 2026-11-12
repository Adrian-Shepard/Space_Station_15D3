#nullable disable
namespace SS15.Client.Core.Tiles
{
    public sealed class WallTile : Tile
    {
        public override TileType Type => TileType.Wall;
        public override bool IsSolid => true;
        public override bool IsOpaque => true;
        public override ChunkSlot GetRenderSlot(GameMap map, int x, int z) => ChunkSlot.Wall;
    }
}