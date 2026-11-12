#nullable disable
namespace SS15.Client.Core.Tiles
{
    public static class TileFactory
    {
        public static Tile Create(TileType type)
        {
            return type switch
            {
                TileType.Floor          => new FloorTile(),
                TileType.Wall           => new WallTile(),
                TileType.ReinforcedWall => new ReinforcedWallTile(),
                TileType.Door           => new DoorTile(),
                TileType.Wire           => new WireTile(),
                TileType.Generator      => new GeneratorTile(),
                TileType.Lamp           => new LampTile(),
                TileType.Space          => new SpaceTile(),
                _                       => new FloorTile(),
            };
        }
    }
}