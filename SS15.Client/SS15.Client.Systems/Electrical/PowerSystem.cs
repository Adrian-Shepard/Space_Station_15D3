using System.Collections.Generic;
using SS15.Client.Core;
namespace SS15.Client.Systems
{
    public class PowerSystem
    {
        private GameMap _map;
        private bool[,] _powered;
        private float _updateTimer = 0f;
        private const float UpdateInterval = 0.5f;

        public void Initialize(GameMap map)
        {
            _map = map;
            _powered = new bool[map.Width, map.Height];
        }

        public void Update(float deltaTime)
        {
            _updateTimer += deltaTime;
            if (_updateTimer < UpdateInterval)
                return;
            _updateTimer = 0f;
            RecalculatePower();
        }

        public bool IsPowered(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= _map.Width || tileY < 0 || tileY >= _map.Height)
                return false;
            return _powered[tileX, tileY];
        }

        private void RecalculatePower()
        {
            var queue = new Queue<(int x, int y)>();
            var visited = new bool[_map.Width, _map.Height];

            // Находим все генераторы и добавляем их в очередь
            for (int y = 0; y < _map.Height; y++)
            {
                for (int x = 0; x < _map.Width; x++)
                {
                    if (_map.GetTile(x, y) == TileType.Generator)
                    {
                        queue.Enqueue((x, y));
                        visited[x, y] = true;
                        _powered[x, y] = true;
                    }
                    else
                    {
                        _powered[x, y] = false;
                    }
                }
            }

            // BFS по проводам и лампам
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || nx >= _map.Width || ny < 0 || ny >= _map.Height)
                        continue;
                    if (visited[nx, ny])
                        continue;

                    TileType tile = _map.GetTile(nx, ny);
                    // Провода и лампы проводят электричество, если есть сосед с питанием
                    if (tile == TileType.Wire || tile == TileType.Lamp)
                    {
                        visited[nx, ny] = true;
                        _powered[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }
    }
}