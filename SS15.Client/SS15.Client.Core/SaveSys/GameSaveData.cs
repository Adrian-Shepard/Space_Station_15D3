using System;

namespace SS15.Client.Core
{
    /// <summary>Сериализуемый снимок игрового состояния. Только DTO, никаких ссылок на движок.</summary>
    public class GameSaveData
    {
        public int Version = 1;
        public string SavedAt = "";

        public int Width;
        public int Height;
        public int TileSize;

        // Тайлы как плоский массив [y * Width + x]
        public int[] Tiles = Array.Empty<int>();

        public ItemStackDto[] GroundItems = Array.Empty<ItemStackDto>();
        public DoorGroupDto[] DoorGroups = Array.Empty<DoorGroupDto>();

        public float PlayerX, PlayerY;
        public float PlayerDirection, PlayerPitch;
        public int PlayerHealth;

        // null в слоте = пусто
        public ItemDto[] Inventory = Array.Empty<ItemDto>();
    }

    public class ItemStackDto
    {
        public int X, Y;
        public ItemDto[] Items = Array.Empty<ItemDto>();
    }

    public class ItemDto
    {
        public int Type;
        public string Name = "";
        public int Quantity;
        public byte[] Color = Array.Empty<byte>();
    }

    public class DoorGroupDto
    {
        public int GroupId;
        public bool IsOpen;
        public int[] TilesX = Array.Empty<int>();
        public int[] TilesY = Array.Empty<int>();
    }
}