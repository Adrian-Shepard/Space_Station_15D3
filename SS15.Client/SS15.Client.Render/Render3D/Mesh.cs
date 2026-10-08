#nullable disable
using Silk.NET.OpenGL;
using System;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public class Mesh : IDisposable
    {
        private GL _gl;
        public uint Vao { get; private set; }
        public uint Vbo { get; private set; }
        public uint Ebo { get; private set; }
        public int IndexCount { get; private set; }

        public Mesh(GL gl, Vertex3D[] vertices, uint[] indices)
        {
            _gl = gl;
            IndexCount = indices.Length;

            Vao = _gl.GenVertexArray();
            Vbo = _gl.GenBuffer();
            Ebo = _gl.GenBuffer();

            _gl.BindVertexArray(Vao);

            unsafe
            {
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
                fixed (Vertex3D* v = vertices)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer,
                        (nuint)(vertices.Length * sizeof(Vertex3D)), v, BufferUsageARB.StaticDraw);
                }

                _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo);
                fixed (uint* i = indices)
                {
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                        (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
                }

                uint stride = (uint)sizeof(Vertex3D);
                _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
                _gl.EnableVertexAttribArray(0);
                _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
                _gl.EnableVertexAttribArray(1);
                _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (void*)(6 * sizeof(float)));
                _gl.EnableVertexAttribArray(2);
            }

            _gl.BindVertexArray(0);

            Logger.Info("Mesh", $"Created VAO={Vao} VBO={Vbo} EBO={Ebo} verts={vertices.Length} indices={indices.Length}");
        }

        public void Draw()
        {
            _gl.BindVertexArray(Vao);
            unsafe
            {
                _gl.DrawElements(PrimitiveType.Triangles, (uint)IndexCount,
                    DrawElementsType.UnsignedInt, (void*)0);
            }
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(Vao);
            _gl.DeleteBuffer(Vbo);
            _gl.DeleteBuffer(Ebo);
        }
    }
}