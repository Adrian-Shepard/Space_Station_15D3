#nullable disable
namespace SS15.Client.Core.Tiles
{
    public sealed class LampTile : Tile
    {
        public override TileType Type => TileType.Lamp;
        public override bool IsConductive => true;
        public override ChunkSlot GetRenderSlot(GameMap map, int x, int z) => ChunkSlot.Floor;
    }
}