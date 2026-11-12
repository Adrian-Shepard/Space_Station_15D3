#nullable disable
namespace SS15.Client.Core.Tiles
{
    public sealed class GeneratorTile : Tile
    {
        public override TileType Type => TileType.Generator;
        public override bool IsSolid => true;
        public override bool IsConductive => true;
        public override ChunkSlot GetRenderSlot(GameMap map, int x, int z) => ChunkSlot.Generator;
    }
}