using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Machinery
{
    public class Generator : TileObjectBase
    {
        public override TileType TileType => TileType.Generator;
        public override string Name => "Generator";
        public override bool IsSolid => false;

        public override void OnPlaced(GameMap map, int x, int y)
        {
            // Здесь можно инициализировать питание
        }
    }
}