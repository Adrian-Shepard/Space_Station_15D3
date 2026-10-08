#nullable disable
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace SS15.Client.Render
{
    public static class TileMeshBuilder
    {
        private const float CapV = 0.4f / 2.4f;
        private const float WireWidth = 0.10f;

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

        public static Mesh BuildCenteredCube(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();

            AddQuad(verts, indices,
                new Vector3( 0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3( 1, 0, 0));
            AddQuad(verts, indices,
                new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3(-1, 0, 0));
            AddQuad(verts, indices,
                new Vector3(-0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3( 0, 1, 0));
            AddQuad(verts, indices,
                new Vector3(-0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0, -1, 0));
            AddQuad(verts, indices,
                new Vector3(-0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3( 0, 0, 1));
            AddQuad(verts, indices,
                new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3( 0, 0, -1));

            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

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

        public static Mesh BuildWireCenter(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();
            float h = WireWidth / 2f;
            AddQuad(verts, indices,
                new Vector3(0.5f - h, 0, 0.5f + h), new Vector3(0.5f + h, 0, 0.5f + h),
                new Vector3(0.5f + h, 0, 0.5f - h), new Vector3(0.5f - h, 0, 0.5f - h),
                new Vector3(0, 1, 0));
            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        public static Mesh BuildWireArmN(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();
            float h = WireWidth / 2f;
            AddQuad(verts, indices,
                new Vector3(0.5f - h, 0, 0.5f - h), new Vector3(0.5f + h, 0, 0.5f - h),
                new Vector3(0.5f + h, 0, 0f), new Vector3(0.5f - h, 0, 0f),
                new Vector3(0, 1, 0));
            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        public static Mesh BuildWireArmS(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();
            float h = WireWidth / 2f;
            AddQuad(verts, indices,
                new Vector3(0.5f - h, 0, 1f), new Vector3(0.5f + h, 0, 1f),
                new Vector3(0.5f + h, 0, 0.5f + h), new Vector3(0.5f - h, 0, 0.5f + h),
                new Vector3(0, 1, 0));
            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        public static Mesh BuildWireArmW(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();
            float h = WireWidth / 2f;
            AddQuad(verts, indices,
                new Vector3(0f, 0, 0.5f + h), new Vector3(0.5f - h, 0, 0.5f + h),
                new Vector3(0.5f - h, 0, 0.5f - h), new Vector3(0f, 0, 0.5f - h),
                new Vector3(0, 1, 0));
            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

        public static Mesh BuildWireArmE(GL gl)
        {
            var verts = new List<Vertex3D>();
            var indices = new List<uint>();
            float h = WireWidth / 2f;
            AddQuad(verts, indices,
                new Vector3(0.5f + h, 0, 0.5f + h), new Vector3(1f, 0, 0.5f + h),
                new Vector3(1f, 0, 0.5f - h), new Vector3(0.5f + h, 0, 0.5f - h),
                new Vector3(0, 1, 0));
            return new Mesh(gl, verts.ToArray(), indices.ToArray());
        }

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

        public static Mesh BuildHighlightBox(GL gl) => BuildUnitCube(gl);

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