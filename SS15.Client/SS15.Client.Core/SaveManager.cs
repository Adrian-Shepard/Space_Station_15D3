using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SS15.Client.Player;

namespace SS15.Client.Core
{
    public static class SaveManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        public static string SaveDirectory =>
            Path.Combine(AppContext.BaseDirectory, "saves");

        public static string DefaultPath =>
            Path.Combine(SaveDirectory, "station.ss15.json");

        public static bool Exists(string path) => File.Exists(path);

        public static void Save(GameMap map, PlayerController player, string path)
        {
            var data = Build(map, player);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(path, json);
        }

        public static GameSaveData Load(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions);
        }

        // ===== Build (runtime → DTO) =====

        private static GameSaveData Build(GameMap map, PlayerController player)
        {
            var data = new GameSaveData
            {
                SavedAt = DateTime.UtcNow.ToString("o"),
                Width = map.Width,
                Height = map.Height,
                TileSize = map.TileSize,
                PlayerX = player.Position.X,
                PlayerY = player.Position.Y,
                PlayerDirection = player.Direction,
                PlayerPitch = player.Pitch,
                PlayerHealth = player.Health,
            };

            // Тайлы
            var tiles = new int[map.Width * map.Height];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    tiles[y * map.Width + x] = (int)map.GetTile(x, y);
            data.Tiles = tiles;

            // Предметы на полу
            var groundList = new List<ItemStackDto>();
            foreach (var kv in map.ItemsOnGround)
            {
                if (kv.Value == null || kv.Value.Count == 0) continue;
                var stack = new ItemStackDto { X = kv.Key.x, Y = kv.Key.y };
                var items = new List<ItemDto>(kv.Value.Count);
                foreach (var item in kv.Value)
                    items.Add(ItemToDto(item));
                stack.Items = items.ToArray();
                groundList.Add(stack);
            }
            data.GroundItems = groundList.ToArray();

            // Двери
            var doorList = new List<DoorGroupDto>();
            foreach (var gkv in map.DoorGroupStates)
            {
                var grp = new DoorGroupDto
                {
                    GroupId = gkv.Key,
                    IsOpen = gkv.Value,
                };
                var xs = new List<int>();
                var ys = new List<int>();
                foreach (var tkv in map.DoorGroups)
                {
                    if (tkv.Value == gkv.Key)
                    {
                        xs.Add(tkv.Key.x);
                        ys.Add(tkv.Key.y);
                    }
                }
                grp.TilesX = xs.ToArray();
                grp.TilesY = ys.ToArray();
                doorList.Add(grp);
            }
            data.DoorGroups = doorList.ToArray();

            // Инвентарь
            var inv = new ItemDto[player.Inventory.Slots.Length];
            for (int i = 0; i < player.Inventory.Slots.Length; i++)
            {
                var item = player.Inventory.Slots[i];
                inv[i] = item != null ? ItemToDto(item) : null;
            }
            data.Inventory = inv;

            return data;
        }

        // ===== Apply (DTO → runtime) =====

        public static void Apply(GameSaveData data, GameMap map, PlayerController player)
        {
            if (data == null) return;

            // Тайлы
            for (int y = 0; y < map.Height && y < data.Height; y++)
                for (int x = 0; x < map.Width && x < data.Width; x++)
                    map.SetTile(x, y, (TileType)data.Tiles[y * data.Width + x]);

            // Предметы на полу
            map.ItemsOnGround.Clear();
            foreach (var stack in data.GroundItems)
            {
                var list = new List<Item>(stack.Items.Length);
                foreach (var dto in stack.Items)
                    list.Add(DtoToItem(dto));
                map.ItemsOnGround[(stack.X, stack.Y)] = list;
            }

            // Двери
            map.DoorGroups.Clear();
            map.DoorGroupStates.Clear();
            int maxId = 0;
            foreach (var grp in data.DoorGroups)
            {
                map.DoorGroupStates[grp.GroupId] = grp.IsOpen;
                for (int i = 0; i < grp.TilesX.Length; i++)
                    map.DoorGroups[(grp.TilesX[i], grp.TilesY[i])] = grp.GroupId;
                if (grp.GroupId > maxId) maxId = grp.GroupId;
            }
            map.SetNextDoorGroupId(maxId + 1);

            // Игрок
            player.Position = new System.Numerics.Vector2(data.PlayerX, data.PlayerY);
            player.Direction = data.PlayerDirection;
            player.Pitch = data.PlayerPitch;
            player.SetHealth(data.PlayerHealth);

            // Инвентарь
            for (int i = 0; i < player.Inventory.Slots.Length; i++)
            {
                if (i < data.Inventory.Length && data.Inventory[i] != null)
                    player.Inventory.Slots[i] = DtoToItem(data.Inventory[i]);
                else
                    player.Inventory.Slots[i] = null;
            }
        }

        // ===== Item ↔ DTO =====

        private static ItemDto ItemToDto(Item item) => new ItemDto
        {
            Type = (int)item.Type,
            Name = item.Name ?? "",
            Quantity = item.Quantity,
            Color = item.Color ?? Array.Empty<byte>(),
        };

        private static Item DtoToItem(ItemDto dto) =>
            new Item((ItemType)dto.Type, dto.Name, dto.Quantity, dto.Color);
    }
}