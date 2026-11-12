#nullable disable
using SS15.Client.Commands;

namespace SS15.Client.Core.Tiles
{
    public sealed class DoorTile : Tile
    {
        public int GroupId;
        public bool IsOpen;

        public override TileType Type => TileType.Door;
        public override bool IsSolid => !IsOpen;
        public override bool IsOpaque => !IsOpen;

        public override ChunkSlot GetRenderSlot(GameMap map, int x, int z)
            => IsOpen ? ChunkSlot.DoorOpen : ChunkSlot.DoorClosed;

        public override void OnPlaced(GameMap map, int x, int z)
        {
            map.DoorGroups[(x, z)] = GroupId;
            if (!map.DoorGroupStates.ContainsKey(GroupId))
                map.DoorGroupStates[GroupId] = false;
        }

        public override void OnInteract(CommandDispatcher commands, int x, int z)
        {
            commands.Dispatch(new ToggleDoorCommand { X = x, Z = z });
        }
    }
}