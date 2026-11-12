#nullable disable
namespace SS15.Client.Core.Tiles
{
    public sealed class SpaceTile : Tile
    {
        public override TileType Type => TileType.Space;
        public override bool IsSolid => true;
        public override bool IsOpaque => true;
    }
}