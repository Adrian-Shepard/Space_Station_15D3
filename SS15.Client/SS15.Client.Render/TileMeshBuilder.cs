#nullable disable
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace SS15.Client.Render
{
    public static class TileMeshBuilder
    {
        // Доля высоты текстуры, которую занимает косяк (0.4 / 2.4 = 1/6).
        private const float CapV = 0.4f / 2.4f;   // ≈ 0.1667

        // ===== Куб (пол/техника) =====
        public static Mesh BuildUnitCube(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            AddQuad(verts, indices,
                new Vector3(1, 0, 0), new Vector3(1, 0, 1),
                new Vector3(1, 1, 1), new Vector3(1, 1, 0),
                new Vector3(1, 0, 0));
            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(0, 0, 0),
                new Vector3(0, 1, 0), new Vector3(0, 1, 1),
                new Vector3(-1, 0, 0));
            AddQuad(verts, indices,
                new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                new Vector3(0, 1, 0));
            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                new Vector3(1, 0, 0), new Vector3(0, 0, 0),
                new Vector3(0, -1, 0));
            AddQuad(verts, indices,
                new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                new Vector3(0, 0, 1));
            AddQuad(verts, indices,
                new Vector3(1, 0, 0), new Vector3(0, 0, 0),
                new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 0, -1));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        // ===== Пол =====
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

        // ===== Лица стен (полная высота 0..1, UV 0..1) =====
        public static Mesh BuildWallFaceN(GL gl) => BuildFace(gl,
            new Vector3(1, 0, 0), new Vector3(0, 0, 0),
            new Vector3(0, 1, 0), new Vector3(1, 1, 0),
            new Vector3(0, 0, -1));

        public static Mesh BuildWallFaceS(GL gl) => BuildFace(gl,
            new Vector3(0, 0, 0), new Vector3(1, 0, 0),
            new Vector3(1, 1, 0), new Vector3(0, 1, 0),
            new Vector3(0, 0, 1));

        public static Mesh BuildWallFaceW(GL gl) => BuildFace(gl,
            new Vector3(0, 0, 1), new Vector3(0, 0, 0),
            new Vector3(0, 1, 0), new Vector3(0, 1, 1),
            new Vector3(-1, 0, 0));

        public static Mesh BuildWallFaceE(GL gl) => BuildFace(gl,
            new Vector3(0, 0, 0), new Vector3(0, 0, 1),
            new Vector3(0, 1, 1), new Vector3(0, 1, 0),
            new Vector3(1, 0, 0));

        // ===== Лица косяка (0..0.4 локально, UV показывает верх 1/6 текстуры) =====
        // UV: низ косяка (y=0)    -> v = CapV   (0.167)
        //      верх косяка (y=0.4) -> v = 0      (самый верх текстуры)

        public static Mesh BuildCapFaceN(GL gl) => BuildCapFace(gl,
            new Vector3(1, 0, 0), new Vector3(0, 0, 0),
            new Vector3(0, 0.4f, 0), new Vector3(1, 0.4f, 0),
            new Vector3(0, 0, -1));

        public static Mesh BuildCapFaceS(GL gl) => BuildCapFace(gl,
            new Vector3(0, 0, 0), new Vector3(1, 0, 0),
            new Vector3(1, 0.4f, 0), new Vector3(0, 0.4f, 0),
            new Vector3(0, 0, 1));

        public static Mesh BuildCapFaceW(GL gl) => BuildCapFace(gl,
            new Vector3(0, 0, 1), new Vector3(0, 0, 0),
            new Vector3(0, 0.4f, 0), new Vector3(0, 0.4f, 1),
            new Vector3(-1, 0, 0));

        public static Mesh BuildCapFaceE(GL gl) => BuildCapFace(gl,
            new Vector3(0, 0, 0), new Vector3(0, 0, 1),
            new Vector3(0, 0.4f, 1), new Vector3(0, 0.4f, 0),
            new Vector3(1, 0, 0));

        // ===== Внутренние хелперы =====

        private static Mesh BuildFace(GL gl,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            // Стандартные UV — вся текстура
            verts.Add(new Vertex3D { Position = a, Normal = normal, TexCoord = new Vector2(0, 1) });
            verts.Add(new Vertex3D { Position = b, Normal = normal, TexCoord = new Vector2(1, 1) });
            verts.Add(new Vertex3D { Position = c, Normal = normal, TexCoord = new Vector2(1, 0) });
            verts.Add(new Vertex3D { Position = d, Normal = normal, TexCoord = new Vector2(0, 0) });

            indices.Add(0); indices.Add(1); indices.Add(2);
            indices.Add(0); indices.Add(2); indices.Add(3);

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        private static Mesh BuildCapFace(GL gl,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            // UV — только верхняя часть текстуры (v: 0.167 -> 0)
            verts.Add(new Vertex3D { Position = a, Normal = normal, TexCoord = new Vector2(0, CapV) });
            verts.Add(new Vertex3D { Position = b, Normal = normal, TexCoord = new Vector2(1, CapV) });
            verts.Add(new Vertex3D { Position = c, Normal = normal, TexCoord = new Vector2(1, 0) });
            verts.Add(new Vertex3D { Position = d, Normal = normal, TexCoord = new Vector2(0, 0) });

            indices.Add(0); indices.Add(1); indices.Add(2);
            indices.Add(0); indices.Add(2); indices.Add(3);

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