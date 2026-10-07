using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Walls
{
    public class WallStandard : TileObjectBase
    {
        public override TileType TileType => TileType.Wall;
        public override string Name => "Standard Wall";
        public override bool IsSolid => true;
    }
}