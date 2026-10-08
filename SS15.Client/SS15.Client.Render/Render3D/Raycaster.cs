using System;
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public struct RaycastHit
    {
        public bool Hit;
        public int X, Z;
        public Vector3 Position;
        public Vector3 Normal;
        public TileType Tile;
        public float Distance;
        public float HitHeight;

        public static RaycastHit None => new RaycastHit { Hit = false, X = -1, Z = -1 };
    }

    public static class Raycaster
    {
        public static float GetTileHeight(TileType tile) => tile switch
        {
            TileType.Wall           => 2.4f,
            TileType.ReinforcedWall => 2.4f,
            TileType.Door           => 2.0f,
            TileType.Generator      => 0.6f,
            TileType.Lamp           => 0.6f,
            TileType.Floor          => 0.08f,
            TileType.Wire           => 0.08f,
            _                       => 0f
        };

        /// DDA по XZ-сетке + Y-slab проверка на каждом тайле.
        /// Полноценный 3D-луч: смотреть вверх — промахнёшься; смотреть в стену — попадёшь.
        public static RaycastHit Cast(GameMap map, Vector3 origin, Vector3 direction, float maxDistance)
        {
            float len = direction.Length();
            if (len < 1e-6f) return RaycastHit.None;
            direction /= len;

            int x = (int)MathF.Floor(origin.X);
            int z = (int)MathF.Floor(origin.Z);

            int stepX = direction.X > 0 ? 1 : (direction.X < 0 ? -1 : 0);
            int stepZ = direction.Z > 0 ? 1 : (direction.Z < 0 ? -1 : 0);

            float tDeltaX = direction.X != 0 ? MathF.Abs(1f / direction.X) : float.PositiveInfinity;
            float tDeltaZ = direction.Z != 0 ? MathF.Abs(1f / direction.Z) : float.PositiveInfinity;

            float tMaxX, tMaxZ;
            if (direction.X > 0) tMaxX = (x + 1 - origin.X) / direction.X;
            else if (direction.X < 0) tMaxX = (x - origin.X) / direction.X;
            else tMaxX = float.PositiveInfinity;

            if (direction.Z > 0) tMaxZ = (z + 1 - origin.Z) / direction.Z;
            else if (direction.Z < 0) tMaxZ = (z - origin.Z) / direction.Z;
            else tMaxZ = float.PositiveInfinity;

            float tEnter = 0f;
            Vector3 lastNormal = new Vector3(-stepX, 0, 0);

            for (int i = 0; i < 512; i++)
            {
                float tExit = MathF.Min(tMaxX, tMaxZ);
                if (tExit > maxDistance) tExit = maxDistance;

                var hit = TryHitTile(map, x, z, origin, direction, tEnter, tExit, lastNormal, maxDistance);
                if (hit.Hit) return hit;

                if (tExit >= maxDistance) break;

                if (tMaxX < tMaxZ)
                {
                    x += stepX;
                    tEnter = tMaxX;
                    tMaxX += tDeltaX;
                    lastNormal = new Vector3(-stepX, 0, 0);
                }
                else
                {
                    z += stepZ;
                    tEnter = tMaxZ;
                    tMaxZ += tDeltaZ;
                    lastNormal = new Vector3(0, 0, -stepZ);
                }

                if (tEnter > maxDistance) break;
            }

            return RaycastHit.None;
        }

        private static RaycastHit TryHitTile(GameMap map, int x, int z,
            Vector3 origin, Vector3 dir, float tEnter, float tExit,
            Vector3 normalXZ, float maxDistance)
        {
            if (x < 0 || x >= map.Width || z < 0 || z >= map.Height)
                return RaycastHit.None;

            var tile = map.GetTile(x, z);
            float height = GetTileHeight(tile);

            // Пол / wire / space: тонкая плоскость на Y=0
            if (tile == TileType.Floor || tile == TileType.Wire || tile == TileType.Space)
            {
                if (dir.Y >= 0) return RaycastHit.None;
                float t = (0f - origin.Y) / dir.Y;
                if (t < tEnter || t > tExit || t < 0 || t > maxDistance) return RaycastHit.None;
                return new RaycastHit
                {
                    Hit = true, X = x, Z = z,
                    Position = origin + dir * t,
                    Normal = new Vector3(0, 1, 0),
                    Tile = tile, Distance = t, HitHeight = 0f
                };
            }

            if (height <= 0f) return RaycastHit.None;

            // Y-координаты входа/выхода луча в XZ-тайл
            float yEnter = origin.Y + dir.Y * tEnter;
            float yExit  = origin.Y + dir.Y * tExit;

            float yMin = MathF.Min(yEnter, yExit);
            float yMax = MathF.Max(yEnter, yExit);

            if (yMax < 0f) return RaycastHit.None;
            if (yMin > height) return RaycastHit.None;

            float tHit = tEnter;
            Vector3 normal = normalXZ;

            if (dir.Y > 0 && yEnter < 0f)
            {
                // Вошли через нижнюю грань
                tHit = tEnter + (0f - yEnter) / dir.Y;
                normal = new Vector3(0, -1, 0);
            }
            else if (dir.Y < 0 && yEnter > height)
            {
                // Вошли через верхнюю грань
                tHit = tEnter + (height - yEnter) / dir.Y;
                normal = new Vector3(0, 1, 0);
            }

            if (tHit > maxDistance) return RaycastHit.None;
            if (tHit < 0f) tHit = 0f;

            return new RaycastHit
            {
                Hit = true, X = x, Z = z,
                Position = origin + dir * tHit,
                Normal = normal,
                Tile = tile, Distance = tHit, HitHeight = origin.Y + dir.Y * tHit
            };
        }
    }
}