#nullable disable

using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Engine.Rendering
{
    public class PrimitiveBatch : IDisposable
    {
        private GL _gl;
        private Shader _shader;
        private uint _vao, _vbo;
        private List<Vertex> _vertices = new();

        private const string VertexShaderSource = @"
#version 460 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec4 aColor;

out vec4 vColor;

uniform mat4 uProjection;
uniform mat4 uView;

void main()
{
    gl_Position = uProjection * uView * vec4(aPosition, 0.0, 1.0);
    vColor = aColor;
}";

        private const string FragmentShaderSource = @"
#version 460 core
in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}";

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector2 Position;
            public Vector4 Color;
        }

        public PrimitiveBatch(GL gl, int maxVertices = 10000)
        {
            _gl = gl;
            _shader = new Shader(gl, VertexShaderSource, FragmentShaderSource);

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            unsafe
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(maxVertices * Marshal.SizeOf<Vertex>()), (void*)0, BufferUsageARB.DynamicDraw);
            }

            uint stride = (uint)Marshal.SizeOf<Vertex>();
            unsafe
            {
                _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
                _gl.EnableVertexAttribArray(0);
                _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, (void*)(2 * sizeof(float)));
                _gl.EnableVertexAttribArray(1);
            }

            _gl.BindVertexArray(0);
        }

        public void Begin(Matrix4x4 projection, Matrix4x4 view)
        {
            _shader.Use();
            _shader.SetMatrix4("uProjection", projection);
            _shader.SetMatrix4("uView", view);
            _vertices.Clear();
        }

        public void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color)
        {
            _vertices.Add(new Vertex { Position = v1, Color = new Vector4(color.R, color.G, color.B, color.A) });
            _vertices.Add(new Vertex { Position = v2, Color = new Vector4(color.R, color.G, color.B, color.A) });
            _vertices.Add(new Vertex { Position = v3, Color = new Vector4(color.R, color.G, color.B, color.A) });
        }

        public void End()
        {
            Flush();
            _gl.BindVertexArray(0);
            _gl.UseProgram(0);
        }

        private unsafe void Flush()
        {
            if (_vertices.Count == 0) return;

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            var vertexArray = _vertices.ToArray();
            fixed (Vertex* ptr = vertexArray)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, IntPtr.Zero, (nuint)(vertexArray.Length * sizeof(Vertex)), ptr);
            }

            _shader.Use();
            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)_vertices.Count);
            _gl.BindVertexArray(0);

            _vertices.Clear();
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _shader.Dispose();
        }
    }
}