#nullable disable
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace SS15.Client.Render
{
    public static class TileMeshBuilder
    {
        // Куб: XZ от 0..1, Y от 0 (низ) до 1 (верх)
        public static Mesh BuildUnitCube(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            // +X
            AddQuad(verts, indices,
                new Vector3(1, 0, 0), new Vector3(1, 0, 1),
                new Vector3(1, 1, 1), new Vector3(1, 1, 0),
                new Vector3(1, 0, 0));

            // -X
            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(0, 0, 0),
                new Vector3(0, 1, 0), new Vector3(0, 1, 1),
                new Vector3(-1, 0, 0));

            // +Y
            AddQuad(verts, indices,
                new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                new Vector3(0, 1, 0));

            // -Y
            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 0),
                new Vector3(0, -1, 0));

            // +Z
            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                new Vector3(0, 0, 1));

            // -Z
            AddQuad(verts, indices,
                new Vector3(1, 0, 0), new Vector3(0, 0, 0),
                new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, -1));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        // Плоский квад пола (XZ), нормаль +Y
        public static Mesh BuildFloorQuad(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 0),
                new Vector3(0, 1, 0));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        private static void AddQuad(List<Vertex3D> verts, List<uint> indices,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            uint b0 = (uint)verts.Count;

            verts.Add(new Vertex3D { Position = a, Normal = normal, TexCoord = new Vector2(0, 1) });
            verts.Add(new Vertex3D { Position = b, Normal = normal, TexCoord = new Vector2(1, 1) });
            verts.Add(new Vertex3D { Position = c, Normal = normal, TexCoord = new Vector2(1, 0) });
            verts.Add(new Vertex3D { Position = d, Normal = normal, TexCoord = new Vector2(0, 0) });

            indices.Add(b0); indices.Add(b0 + 1); indices.Add(b0 + 2);
            indices.Add(b0); indices.Add(b0 + 2); indices.Add(b0 + 3);
        }
    }
}