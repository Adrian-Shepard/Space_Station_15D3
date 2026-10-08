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
        public Dictionary<(int x, int y), int> DoorGroups { get; } = new();
        public Dictionary<int, bool> DoorGroupStates { get; } = new();
        private int _nextDoorGroupId = 1;

        public event Action<int, int> OnTileChanged;

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
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            if (_tiles[y, x] == type) return;

            _tiles[y, x] = type;
            OnTileChanged?.Invoke(x, y);
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
            }
            return true;
        }

        public int NewDoorGroup() => _nextDoorGroupId++;

        public void SetNextDoorGroupId(int id)
        {
            _nextDoorGroupId = id < 1 ? 1 : id;
        }

        public void RegisterDoorTile(int x, int y, int groupId)
        {
            DoorGroups[(x, y)] = groupId;
            if (!DoorGroupStates.ContainsKey(groupId))
                DoorGroupStates[groupId] = false;
            OnTileChanged?.Invoke(x, y);
        }

        public int GetDoorGroupId(int x, int y)
        {
            return DoorGroups.TryGetValue((x, y), out int id) ? id : -1;
        }

        public bool IsDoorOpen(int x, int y)
        {
            if (DoorGroups.TryGetValue((x, y), out int gid))
                return DoorGroupStates.TryGetValue(gid, out bool open) && open;
            return false;
        }

        public void SetDoorOpen(int x, int y, bool open)
        {
            if (!DoorGroups.TryGetValue((x, y), out int gid)) return;
            if (DoorGroupStates.TryGetValue(gid, out bool cur) && cur == open) return;

            DoorGroupStates[gid] = open;

            foreach (var kv in DoorGroups)
                if (kv.Value == gid)
                    OnTileChanged?.Invoke(kv.Key.x, kv.Key.y);
        }

        public List<(int x, int y)> GetDoorTilesInGroup(int groupId)
        {
            var list = new List<(int x, int y)>();
            foreach (var kvp in DoorGroups)
                if (kvp.Value == groupId)
                    list.Add(kvp.Key);
            return list;
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