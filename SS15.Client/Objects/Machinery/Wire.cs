using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Machinery
{
    public class Wire : TileObjectBase
    {
        public override TileType TileType => TileType.Wire;
        public override string Name => "Wire";
        public override bool IsSolid => false;
    }
}