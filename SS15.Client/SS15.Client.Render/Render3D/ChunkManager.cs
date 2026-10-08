#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public class ChunkManager
    {
        private readonly GL _gl;
        private readonly GameMap _map;
        private readonly Texture2D[] _textures;
        private readonly Dictionary<(int, int), TileChunk> _chunks = new();

        public int LastDrawnChunks { get; private set; }

        public ChunkManager(GL gl, GameMap map, Texture2D[] textures)
        {
            _gl = gl;
            _map = map;
            _textures = textures;
            map.OnTileChanged += OnTileChanged;
        }

        public void DrawVisible(MeshRenderer renderer, Camera3D camera, int viewRadius)
        {
            int cx = (int)MathF.Floor(camera.Position.X);
            int cz = (int)MathF.Floor(camera.Position.Z);

            float fwdX = MathF.Cos(camera.Yaw);
            float fwdZ = MathF.Sin(camera.Yaw);
            float len = MathF.Sqrt(fwdX * fwdX + fwdZ * fwdZ);
            if (len > 0.001f) { fwdX /= len; fwdZ /= len; }

            int c0x = (cx - viewRadius) / TileChunk.Size;
            int c1x = (cx + viewRadius) / TileChunk.Size;
            int c0z = (cz - viewRadius) / TileChunk.Size;
            int c1z = (cz + viewRadius) / TileChunk.Size;

            if (c0x < 0) c0x = 0;
            if (c0z < 0) c0z = 0;

            int maxCx = (_map.Width - 1) / TileChunk.Size;
            int maxCz = (_map.Height - 1) / TileChunk.Size;
            if (c1x > maxCx) c1x = maxCx;
            if (c1z > maxCz) c1z = maxCz;

            int drawn = 0;

            for (int ccz = c0z; ccz <= c1z; ccz++)
            {
                for (int ccx = c0x; ccx <= c1x; ccx++)
                {
                    var chunk = GetChunk(ccx, ccz);

                    int xMin = chunk.WorldX0;
                    int xMax = chunk.WorldX0 + TileChunk.Size;
                    int zMin = chunk.WorldZ0;
                    int zMax = chunk.WorldZ0 + TileChunk.Size;

                    bool visible;

                    // Чанк, в котором стоит камера — всегда рисуем
                    if (cx >= xMin && cx < xMax && cz >= zMin && cz < zMax)
                    {
                        visible = true;
                    }
                    else
                    {
                        visible = IsChunkVisible(cx, cz, fwdX, fwdZ, xMin, zMin, xMax, zMax);
                    }

                    if (!visible) continue;

                    if (chunk.Dirty) chunk.Rebuild(_gl, _map);
                    if (chunk.Empty) continue;

                    chunk.Draw(renderer, _textures);
                    drawn++;
                }
            }

            LastDrawnChunks = drawn;
        }

        private static bool IsChunkVisible(int camX, int camZ, float fwdX, float fwdZ,
            int xMin, int zMin, int xMax, int zMax)
        {
            if (IsPointVisible(camX, camZ, fwdX, fwdZ, xMin, zMin)) return true;
            if (IsPointVisible(camX, camZ, fwdX, fwdZ, xMax, zMin)) return true;
            if (IsPointVisible(camX, camZ, fwdX, fwdZ, xMin, zMax)) return true;
            if (IsPointVisible(camX, camZ, fwdX, fwdZ, xMax, zMax)) return true;

            float mx = (xMin + xMax) * 0.5f;
            float mz = (zMin + zMax) * 0.5f;
            if (IsPointVisible(camX, camZ, fwdX, fwdZ, mx, mz)) return true;

            return false;
        }

        private static bool IsPointVisible(int camX, int camZ, float fwdX, float fwdZ, float px, float pz)
        {
            float dx = px - camX;
            float dz = pz - camZ;
            float d2 = dx * dx + dz * dz;

            // Близко — не отсекаем
            if (d2 < 144f) return true;

            float dInv = 1f / MathF.Sqrt(d2);
            float dotN = (dx * fwdX + dz * fwdZ) * dInv;

            // Мягкий порог — отсекаем только чётко позади
            return dotN > -0.3f;
        }

        private TileChunk GetChunk(int ccx, int ccz)
        {
            if (!_chunks.TryGetValue((ccx, ccz), out var c))
            {
                c = new TileChunk(ccx, ccz);
                _chunks[(ccx, ccz)] = c;
            }
            return c;
        }

        private void OnTileChanged(int x, int z)
        {
            int ccx = x / TileChunk.Size;
            int ccz = z / TileChunk.Size;

            MarkDirty(ccx, ccz);
            if (x % TileChunk.Size == 0) MarkDirty(ccx - 1, ccz);
            if (x % TileChunk.Size == TileChunk.Size - 1) MarkDirty(ccx + 1, ccz);
            if (z % TileChunk.Size == 0) MarkDirty(ccx, ccz - 1);
            if (z % TileChunk.Size == TileChunk.Size - 1) MarkDirty(ccx, ccz + 1);
        }

        private void MarkDirty(int ccx, int ccz)
        {
            if (ccx < 0 || ccz < 0) return;
            if (_chunks.TryGetValue((ccx, ccz), out var c))
                c.Dirty = true;
        }

        public void Dispose()
        {
            foreach (var c in _chunks.Values) c.Dispose();
            _chunks.Clear();
        }
    }
}