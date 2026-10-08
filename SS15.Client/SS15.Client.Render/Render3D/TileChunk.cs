#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using SS15.Client.Core;
using Engine.Rendering;

namespace SS15.Client.Render
{
    public class TileChunk
    {
        public const int Size = 16;

        public int ChunkX { get; }
        public int ChunkZ { get; }
        public int WorldX0 => ChunkX * Size;
        public int WorldZ0 => ChunkZ * Size;
        public bool Dirty { get; set; } = true;
        public bool Empty { get; private set; } = true;

        // 0=floor, 1=wall(+cap), 2=door_open, 3=door_closed, 4=generator
        private readonly Mesh[] _meshes = new Mesh[5];

        private const float WallH = 2.4f;
        private const float DoorH = 2.4f;

        public TileChunk(int cx, int cz)
        {
            ChunkX = cx;
            ChunkZ = cz;
        }

        public void Rebuild(GL gl, GameMap map)
        {
            var v = new List<Vertex3D>[5];
            var idx = new List<uint>[5];
            for (int i = 0; i < 5; i++) { v[i] = new List<Vertex3D>(); idx[i] = new List<uint>(); }

            int x0 = WorldX0, z0 = WorldZ0;
            int x1 = Math.Min(x0 + Size, map.Width);
            int z1 = Math.Min(z0 + Size, map.Height);

            for (int z = z0; z < z1; z++)
            {
                for (int x = x0; x < x1; x++)
                {
                    var tile = map.GetTile(x, z);
                    float fx = x, fz = z;

                    if (tile != TileType.Space)
                        AddQuad(v[0], idx[0],
                            new Vector3(fx, 0, fz + 1),
                            new Vector3(fx + 1, 0, fz + 1),
                            new Vector3(fx + 1, 0, fz),
                            new Vector3(fx, 0, fz),
                            new Vector3(0, 1, 0));

                    if (tile == TileType.Wall || tile == TileType.ReinforcedWall)
                    {
                        if (!IsWall(map, x, z - 1)) AddQuad(v[1], idx[1],
                            new Vector3(fx + 1, 0, fz), new Vector3(fx, 0, fz),
                            new Vector3(fx, WallH, fz), new Vector3(fx + 1, WallH, fz),
                            new Vector3(0, 0, -1));
                        if (!IsWall(map, x, z + 1)) AddQuad(v[1], idx[1],
                            new Vector3(fx, 0, fz + 1), new Vector3(fx + 1, 0, fz + 1),
                            new Vector3(fx + 1, WallH, fz + 1), new Vector3(fx, WallH, fz + 1),
                            new Vector3(0, 0, 1));
                        if (!IsWall(map, x - 1, z)) AddQuad(v[1], idx[1],
                            new Vector3(fx, 0, fz + 1), new Vector3(fx, 0, fz),
                            new Vector3(fx, WallH, fz), new Vector3(fx, WallH, fz + 1),
                            new Vector3(-1, 0, 0));
                        if (!IsWall(map, x + 1, z)) AddQuad(v[1], idx[1],
                            new Vector3(fx + 1, 0, fz), new Vector3(fx + 1, 0, fz + 1),
                            new Vector3(fx + 1, WallH, fz + 1), new Vector3(fx + 1, WallH, fz),
                            new Vector3(1, 0, 0));
                    }
                    else if (tile == TileType.Door)
                    {
                        bool open = map.IsDoorOpen(x, z);
                        int slot = open ? 2 : 3;
                        AddCube(v[slot], idx[slot], fx, 0, fz, 1f, DoorH, 1f);
                    }
                    else if (tile == TileType.Generator)
                    {
                        AddCube(v[4], idx[4], fx + 0.1f, 0, fz + 0.1f, 0.8f, 0.6f, 0.8f);
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (_meshes[i] != null) { _meshes[i].Dispose(); _meshes[i] = null; }
                if (v[i].Count > 0)
                    _meshes[i] = new Mesh(gl, v[i].ToArray(), idx[i].ToArray());
            }

            Empty = true;
            for (int i = 0; i < 5; i++)
                if (_meshes[i] != null) { Empty = false; break; }

            Dirty = false;
        }

        public void Draw(MeshRenderer renderer, Texture2D[] textures)
        {
            for (int i = 0; i < 5; i++)
                if (_meshes[i] != null)
                    renderer.Draw(_meshes[i], textures[i], Vector3.Zero, Vector3.One);
        }

        public void Dispose()
        {
            for (int i = 0; i < 5; i++) { _meshes[i]?.Dispose(); _meshes[i] = null; }
        }

        private static bool IsWall(GameMap map, int x, int z)
        {
            if (x < 0 || x >= map.Width || z < 0 || z >= map.Height) return false;
            var t = map.GetTile(x, z);
            return t == TileType.Wall || t == TileType.ReinforcedWall;
        }

        private static void AddQuad(List<Vertex3D> verts, List<uint> idx,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            uint b0 = (uint)verts.Count;
            verts.Add(new Vertex3D { Position = a, Normal = normal, TexCoord = new Vector2(0, 1) });
            verts.Add(new Vertex3D { Position = b, Normal = normal, TexCoord = new Vector2(1, 1) });
            verts.Add(new Vertex3D { Position = c, Normal = normal, TexCoord = new Vector2(1, 0) });
            verts.Add(new Vertex3D { Position = d, Normal = normal, TexCoord = new Vector2(0, 0) });
            idx.Add(b0); idx.Add(b0 + 1); idx.Add(b0 + 2);
            idx.Add(b0); idx.Add(b0 + 2); idx.Add(b0 + 3);
        }

        private static void AddCube(List<Vertex3D> verts, List<uint> idx,
            float x, float y, float z, float sx, float sy, float sz)
        {
            AddQuad(verts, idx,
                new Vector3(x + sx, y, z), new Vector3(x + sx, y, z + sz),
                new Vector3(x + sx, y + sy, z + sz), new Vector3(x + sx, y + sy, z),
                new Vector3(1, 0, 0));
            AddQuad(verts, idx,
                new Vector3(x, y, z + sz), new Vector3(x, y, z),
                new Vector3(x, y + sy, z), new Vector3(x, y + sy, z + sz),
                new Vector3(-1, 0, 0));
            AddQuad(verts, idx,
                new Vector3(x, y + sy, z), new Vector3(x + sx, y + sy, z),
                new Vector3(x + sx, y + sy, z + sz), new Vector3(x, y + sy, z + sz),
                new Vector3(0, 1, 0));
            AddQuad(verts, idx,
                new Vector3(x, y, z + sz), new Vector3(x + sx, y, z + sz),
                new Vector3(x + sx, y, z), new Vector3(x, y, z),
                new Vector3(0, -1, 0));
            AddQuad(verts, idx,
                new Vector3(x, y, z + sz), new Vector3(x + sx, y, z + sz),
                new Vector3(x + sx, y + sy, z + sz), new Vector3(x, y + sy, z + sz),
                new Vector3(0, 0, 1));
            AddQuad(verts, idx,
                new Vector3(x + sx, y, z), new Vector3(x, y, z),
                new Vector3(x, y + sy, z), new Vector3(x + sx, y + sy, z),
                new Vector3(0, 0, -1));
        }
    }
}