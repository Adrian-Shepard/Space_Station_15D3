using SS15.Client.Core;

namespace SS15.Client.Objects
{
    public abstract class TileObjectBase
    {
        public abstract TileType TileType { get; }
        public abstract string Name { get; }
        public abstract bool IsSolid { get; }

        // Вызывается при установке на карту
        public virtual void OnPlaced(GameMap map, int x, int y) { }

        // Вызывается при удалении с карты
        public virtual void OnRemoved(GameMap map, int x, int y) { }
    }
}