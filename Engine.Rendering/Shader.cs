#nullable disable

using Silk.NET.OpenGL;
using System;
using System.Numerics; // добавлено для Matrix4x4

namespace Engine.Rendering
{
    public class Shader : IDisposable
    {
        public uint Program { get; private set; }
        private GL _gl;

        public Shader(GL gl, string vertexSource, string fragmentSource)
        {
            _gl = gl;
            uint vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            Program = _gl.CreateProgram();
            _gl.AttachShader(Program, vertexShader);
            _gl.AttachShader(Program, fragmentShader);
            _gl.LinkProgram(Program);

            _gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string infoLog = _gl.GetProgramInfoLog(Program);
                throw new Exception($"Program link error: {infoLog}");
            }

            _gl.DetachShader(Program, vertexShader);
            _gl.DetachShader(Program, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);
        }

        private uint CompileShader(ShaderType type, string source)
        {
            uint shader = _gl.CreateShader(type);
            _gl.ShaderSource(shader, source);
            _gl.CompileShader(shader);
            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = _gl.GetShaderInfoLog(shader);
                throw new Exception($"Shader compile error ({type}): {infoLog}");
            }
            return shader;
        }

        public void Use() => _gl.UseProgram(Program);

        public void SetMatrix4(string name, Matrix4x4 matrix)
        {
            int location = _gl.GetUniformLocation(Program, name);
            if (location != -1)
            {
                // Передаём матрицу как массив float (column-major)
                float[] values = new float[]
                {
                    matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                    matrix.M41, matrix.M42, matrix.M43, matrix.M44
                };
                _gl.UniformMatrix4(location, 1, false, values);
            }
        }

        public void SetInt(string name, int value)
        {
            int location = _gl.GetUniformLocation(Program, name);
            if (location != -1)
                _gl.Uniform1(location, value);
        }

        public void Dispose()
        {
            if (Program != 0)
                _gl.DeleteProgram(Program);
            Program = 0;
        }
    }
}