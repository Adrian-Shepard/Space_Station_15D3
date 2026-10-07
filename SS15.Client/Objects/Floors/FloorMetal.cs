using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Floors
{
    public class FloorMetal : TileObjectBase
    {
        public override TileType TileType => TileType.MetalFloor;
        public override string Name => "Metal Floor";
        public override bool IsSolid => false;
    }
}