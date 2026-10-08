#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public class ShaderViewModel : IDisposable
    {
        public uint Program { get; private set; }
        private GL _gl;
        private readonly Dictionary<string, int> _uniformCache = new(8);
        private readonly float[] _matrixBuffer = new float[16];

        private const string VertSrc = @"#version 460 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

out vec2 vTexCoord;
out vec3 vNormal;

uniform mat4 uModel;
uniform mat4 uProjection;

void main()
{
    vTexCoord = aTexCoord;
    vNormal = aNormal;
    gl_Position = uProjection * uModel * vec4(aPosition, 1.0);
}";

        private const string FragSrc = @"#version 460 core
in vec2 vTexCoord;
in vec3 vNormal;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec4 uTint;

void main()
{
    vec4 texColor = texture(uTexture, vTexCoord);
    if (texColor.a < 0.05) discard;

    float shade = 0.75 + 0.25 * max(vNormal.y, 0.0);
    FragColor = vec4(texColor.rgb * uTint.rgb * shade, texColor.a * uTint.a);
}";

        public ShaderViewModel(GL gl)
        {
            _gl = gl;
            uint vs = CompileShader(ShaderType.VertexShader, VertSrc);
            uint fs = CompileShader(ShaderType.FragmentShader, FragSrc);

            Program = _gl.CreateProgram();
            _gl.AttachShader(Program, vs);
            _gl.AttachShader(Program, fs);
            _gl.LinkProgram(Program);
            _gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string log = _gl.GetProgramInfoLog(Program);
                Logger.Error("ShaderViewModel", "Link error: " + log);
                throw new Exception("Viewmodel shader link error: " + log);
            }

            _gl.DetachShader(Program, vs);
            _gl.DetachShader(Program, fs);
            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);

            Logger.Info("ShaderViewModel", $"Program linked. Id={Program}");
        }

        private uint CompileShader(ShaderType type, string src)
        {
            uint s = _gl.CreateShader(type);
            _gl.ShaderSource(s, src);
            _gl.CompileShader(s);
            _gl.GetShader(s, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string log = _gl.GetShaderInfoLog(s);
                Logger.Error("ShaderViewModel", $"Compile error ({type}): {log}");
                throw new Exception($"Viewmodel shader compile error ({type}): {log}");
            }
            return s;
        }

        private int GetLoc(string name)
        {
            if (_uniformCache.TryGetValue(name, out int loc)) return loc;
            loc = _gl.GetUniformLocation(Program, name);
            _uniformCache[name] = loc;
            return loc;
        }

        public void Use() => _gl.UseProgram(Program);

        public void SetMatrix4(string name, Matrix4x4 m)
        {
            int loc = GetLoc(name);
            if (loc < 0) return;

            _matrixBuffer[0]  = m.M11; _matrixBuffer[1]  = m.M12;
            _matrixBuffer[2]  = m.M13; _matrixBuffer[3]  = m.M14;
            _matrixBuffer[4]  = m.M21; _matrixBuffer[5]  = m.M22;
            _matrixBuffer[6]  = m.M23; _matrixBuffer[7]  = m.M24;
            _matrixBuffer[8]  = m.M31; _matrixBuffer[9]  = m.M32;
            _matrixBuffer[10] = m.M33; _matrixBuffer[11] = m.M34;
            _matrixBuffer[12] = m.M41; _matrixBuffer[13] = m.M42;
            _matrixBuffer[14] = m.M43; _matrixBuffer[15] = m.M44;

            unsafe
            {
                fixed (float* p = _matrixBuffer)
                    _gl.UniformMatrix4(loc, 1, false, p);
            }
        }

        public void SetVector4(string name, Vector4 v)
        {
            int loc = GetLoc(name);
            if (loc < 0) return;
            _gl.Uniform4(loc, v.X, v.Y, v.Z, v.W);
        }

        public void SetInt(string name, int v)
        {
            int loc = GetLoc(name);
            if (loc < 0) return;
            _gl.Uniform1(loc, v);
        }

        public void Dispose()
        {
            if (Program != 0) _gl.DeleteProgram(Program);
            Program = 0;
            _uniformCache.Clear();
        }
    }
}