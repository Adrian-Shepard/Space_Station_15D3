namespace SS15.Client.Core
{
    public enum TileType
    {
        Floor = 0,
        Wall = 1,
        MetalFloor = 2,      // не используется, оставлено для совместимости
        DirtyFloor = 3,      // не используется
        ReinforcedWall = 4,
        Wire = 5,
        Generator = 6,
        Lamp = 7,
        Door = 8,
        Space = 9            // добавлено
    }
}