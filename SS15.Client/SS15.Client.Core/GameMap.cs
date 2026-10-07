using System;
using System.Collections.Generic;
using System.Numerics;

namespace SS15.Client.Core
{
    public class GameMap
    {
        public int Width { get; }
        public int Height { get; }
        public int TileSize { get; }

        private TileType[,] _tiles;
        public Dictionary<(int x, int y), List<Item>> ItemsOnGround { get; } = new();
        public Dictionary<(int x, int y), bool> DoorStates { get; } = new();

        public GameMap(int width, int height, int tileSize)
        {
            Width = width;
            Height = height;
            TileSize = tileSize;
            _tiles = new TileType[height, width];
        }

        public void Fill(TileType type)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _tiles[y, x] = type;
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                _tiles[y, x] = type;
        }

        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return TileType.Wall;
            return _tiles[y, x];
        }

        public bool IsWalkable(Vector2 position, float playerHalf)
        {
            Vector2[] corners =
            {
                new Vector2(position.X - playerHalf, position.Y - playerHalf),
                new Vector2(position.X + playerHalf, position.Y - playerHalf),
                new Vector2(position.X - playerHalf, position.Y + playerHalf),
                new Vector2(position.X + playerHalf, position.Y + playerHalf)
            };

            foreach (var corner in corners)
            {
                int tileX = (int)(corner.X / TileSize);
                int tileY = (int)(corner.Y / TileSize);
                TileType tile = GetTile(tileX, tileY);
                if (tile == TileType.Wall || tile == TileType.ReinforcedWall)
                    return false;
                if (tile == TileType.Door && !IsDoorOpen(tileX, tileY))
                    return false;
                // Space проходим (урон будет от HealthSystem)
            }
            return true;
        }

        public bool IsDoorOpen(int x, int y)
        {
            return DoorStates.TryGetValue((x, y), out bool open) && open;
        }

        public void SetDoorOpen(int x, int y, bool open)
        {
            DoorStates[(x, y)] = open;
        }

        public void AddItemToTile(int x, int y, Item item)
        {
            if (!ItemsOnGround.ContainsKey((x, y)))
                ItemsOnGround[(x, y)] = new List<Item>();
            ItemsOnGround[(x, y)].Add(item);
        }

        public List<Item> GetItemsAtTile(int x, int y)
        {
            ItemsOnGround.TryGetValue((x, y), out var list);
            return list;
        }

        public void RemoveItemAtTile(int x, int y, Item item)
        {
            if (ItemsOnGround.TryGetValue((x, y), out var list))
            {
                list.Remove(item);
                if (list.Count == 0)
                    ItemsOnGround.Remove((x, y));
            }
        }
    }
}