using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Machinery
{
    public class Lamp : TileObjectBase
    {
        public override TileType TileType => TileType.Lamp;
        public override string Name => "Lamp";
        public override bool IsSolid => false;
    }
}