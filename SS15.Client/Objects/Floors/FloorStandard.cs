using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Floors
{
    public class FloorStandard : TileObjectBase
    {
        public override TileType TileType => TileType.Floor;
        public override string Name => "Floor";
        public override bool IsSolid => false;
    }
}