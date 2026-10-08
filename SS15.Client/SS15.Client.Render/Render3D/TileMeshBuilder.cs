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

        // Толщина провода — 0.10 юнита в мире.
        private const float WireWidth = 0.10f;

        // ===== Куб (пол/техника/подсветка) =====
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

        // ===== Провод: центр + 4 рукава =====
        // Центр — квадрат WireWidth × WireWidth в центре тайла.
        // Рукав — от края центра до края тайла, толщина WireWidth.

        public static Mesh BuildWireCenter(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            float h = WireWidth / 2f;
            float x0 = 0.5f - h, x1 = 0.5f + h;
            float z0 = 0.5f - h, z1 = 0.5f + h;

            AddQuad(verts, indices,
                new Vector3(x0, 0, z1), new Vector3(x1, 0, z1),
                new Vector3(x1, 0, z0), new Vector3(x0, 0, z0),
                new Vector3(0, 1, 0));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        /// Рукав на север (−Z). X в [0.5−h, 0.5+h], Z в [0, 0.5−h].
        public static Mesh BuildWireArmN(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            float h = WireWidth / 2f;
            float x0 = 0.5f - h, x1 = 0.5f + h;
            float z0 = 0f, z1 = 0.5f - h;

            AddQuad(verts, indices,
                new Vector3(x0, 0, z1), new Vector3(x1, 0, z1),
                new Vector3(x1, 0, z0), new Vector3(x0, 0, z0),
                new Vector3(0, 1, 0));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        /// Рукав на юг (+Z). X в [0.5−h, 0.5+h], Z в [0.5+h, 1].
        public static Mesh BuildWireArmS(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            float h = WireWidth / 2f;
            float x0 = 0.5f - h, x1 = 0.5f + h;
            float z0 = 0.5f + h, z1 = 1f;

            AddQuad(verts, indices,
                new Vector3(x0, 0, z1), new Vector3(x1, 0, z1),
                new Vector3(x1, 0, z0), new Vector3(x0, 0, z0),
                new Vector3(0, 1, 0));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        /// Рукав на запад (−X). X в [0, 0.5−h], Z в [0.5−h, 0.5+h].
        public static Mesh BuildWireArmW(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            float h = WireWidth / 2f;
            float x0 = 0f, x1 = 0.5f - h;
            float z0 = 0.5f - h, z1 = 0.5f + h;

            AddQuad(verts, indices,
                new Vector3(x0, 0, z1), new Vector3(x1, 0, z1),
                new Vector3(x1, 0, z0), new Vector3(x0, 0, z0),
                new Vector3(0, 1, 0));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        /// Рукав на восток (+X). X в [0.5+h, 1], Z в [0.5−h, 0.5+h].
        public static Mesh BuildWireArmE(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            float h = WireWidth / 2f;
            float x0 = 0.5f + h, x1 = 1f;
            float z0 = 0.5f - h, z1 = 0.5f + h;

            AddQuad(verts, indices,
                new Vector3(x0, 0, z1), new Vector3(x1, 0, z1),
                new Vector3(x1, 0, z0), new Vector3(x0, 0, z0),
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

        // ===== Лица косяка =====
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

        // ===== Подсветка =====
        public static Mesh BuildHighlightBox(GL gl) => BuildUnitCube(gl);

        // ===== Внутренние хелперы =====

        private static Mesh BuildFace(GL gl,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

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