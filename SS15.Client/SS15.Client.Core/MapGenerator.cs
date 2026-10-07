using System;
using SS15.Client.Core;

namespace SS15.Client.Core
{
    public static class MapGenerator
    {
        private const int MapWidth = 255;
        private const int MapHeight = 255;
        private const int TileSize = 32;

        public static GameMap Generate()
        {
            var map = new GameMap(MapWidth, MapHeight, TileSize);
            Random rng = new Random(987654);

            // Заполняем всю карту обычным полом (без MetalFloor и DirtyFloor)
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    map.SetTile(x, y, TileType.Floor);
                }
            }

            // Внешние стены
            for (int x = 0; x < MapWidth; x++)
            {
                map.SetTile(x, 0, TileType.Wall);
                map.SetTile(x, MapHeight - 1, TileType.Wall);
            }
            for (int y = 0; y < MapHeight; y++)
            {
                map.SetTile(0, y, TileType.Wall);
                map.SetTile(MapWidth - 1, y, TileType.Wall);
            }

            GenerateRooms(map, rng);
            return map;
        }

        private static void GenerateRooms(GameMap map, Random rng)
        {
            int roomCount = 20;
            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = rng.Next(6, 15);
                int roomHeight = rng.Next(6, 12);
                int startX = rng.Next(5, MapWidth - roomWidth - 5);
                int startY = rng.Next(5, MapHeight - roomHeight - 5);

                for (int y = startY; y < startY + roomHeight; y++)
                    for (int x = startX; x < startX + roomWidth; x++)
                        map.SetTile(x, y, TileType.Floor);

                for (int x = startX; x < startX + roomWidth; x++)
                {
                    map.SetTile(x, startY, TileType.Wall);
                    map.SetTile(x, startY + roomHeight - 1, TileType.Wall);
                }
                for (int y = startY; y < startY + roomHeight; y++)
                {
                    map.SetTile(startX, y, TileType.Wall);
                    map.SetTile(startX + roomWidth - 1, y, TileType.Wall);
                }

                int doorX = startX + roomWidth / 2;
                int doorY = startY;
                map.SetTile(doorX, doorY, TileType.Floor);
            }

            AddWallRectangle(map, 50, 50, 10, 5);
            AddWallRectangle(map, 120, 80, 8, 8);
            AddWallRectangle(map, 180, 150, 12, 6);
        }

        private static void AddWallRectangle(GameMap map, int startX, int startY, int width, int height)
        {
            for (int y = startY; y < startY + height; y++)
                for (int x = startX; x < startX + width; x++)
                    map.SetTile(x, y, TileType.Wall);
        }
    }
}