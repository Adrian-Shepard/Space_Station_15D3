using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Walls
{
    public class WallReinforced : TileObjectBase
    {
        public override TileType TileType => TileType.ReinforcedWall;
        public override string Name => "Reinforced Wall";
        public override bool IsSolid => true;
    }
}