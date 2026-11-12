#nullable disable
using SS15.Client.Commands;

namespace SS15.Client.Core.Tiles
{
    public enum ChunkSlot
    {
        None = -1,
        Floor = 0,
        Wall = 1,
        DoorOpen = 2,
        DoorClosed = 3,
        Generator = 4,
    }

    public abstract class Tile
    {
        public abstract TileType Type { get; }

        public virtual bool IsSolid => false;
        public virtual bool IsOpaque => false;
        public virtual bool IsConductive => false;

        public virtual void OnPlaced(GameMap map, int x, int z) { }
        public virtual void OnRemoved(GameMap map, int x, int z) { }
        public virtual void Update(GameMap map, int x, int z, float dt) { }

        public virtual void OnInteract(CommandDispatcher commands, int x, int z) { }

        public virtual ChunkSlot GetRenderSlot(GameMap map, int x, int z) => ChunkSlot.None;
    }
}