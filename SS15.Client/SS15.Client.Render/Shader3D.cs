#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public class Shader3D : IDisposable
    {
        public uint Program { get; private set; }
        private GL _gl;

        private const string VertSrc = @"
#version 460 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

out vec3 vNormal;
out vec2 vTexCoord;
out vec3 vWorldPos;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    vec4 world = uModel * vec4(aPosition, 1.0);
    vWorldPos = world.xyz;
    vNormal = mat3(uModel) * aNormal;
    vTexCoord = aTexCoord;
    gl_Position = uProjection * uView * world;
}";

        private const string FragSrc = @"
#version 460 core
in vec3 vNormal;
in vec2 vTexCoord;
in vec3 vWorldPos;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec3 uLightDir;
uniform vec3 uAmbientColor;
uniform vec3 uLightColor;
uniform vec3 uViewPos;

void main()
{
    vec4 texColor = texture(uTexture, vTexCoord);
    if (texColor.a < 0.05) discard;

    vec3 N = normalize(vNormal);
    vec3 L = normalize(-uLightDir);
    float diff = max(dot(N, L), 0.0);
    vec3 lighting = uAmbientColor + uLightColor * diff;

    vec3 V = normalize(uViewPos - vWorldPos);
    vec3 H = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), 32.0) * 0.15;
    lighting += vec3(spec);

    FragColor = vec4(texColor.rgb * lighting, texColor.a);
}";

        public Shader3D(GL gl)
        {
            _gl = gl;
            Logger.Info("Shader3D", "Compiling vertex shader...");
            uint vs = CompileShader(ShaderType.VertexShader, VertSrc);

            Logger.Info("Shader3D", "Compiling fragment shader...");
            uint fs = CompileShader(ShaderType.FragmentShader, FragSrc);

            Logger.Info("Shader3D", "Linking program...");
            Program = _gl.CreateProgram();
            _gl.AttachShader(Program, vs);
            _gl.AttachShader(Program, fs);
            _gl.LinkProgram(Program);
            _gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string log = _gl.GetProgramInfoLog(Program);
                Logger.Error("Shader3D", "Program link error: " + log);
                throw new Exception("3D shader link error: " + log);
            }

            _gl.DetachShader(Program, vs);
            _gl.DetachShader(Program, fs);
            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);

            Logger.Info("Shader3D", $"Program linked OK. Id={Program}");

            // Проверяем все uniform
            CheckUniform("uModel");
            CheckUniform("uView");
            CheckUniform("uProjection");
            CheckUniform("uTexture");
            CheckUniform("uLightDir");
            CheckUniform("uAmbientColor");
            CheckUniform("uLightColor");
            CheckUniform("uViewPos");
        }

        private void CheckUniform(string name)
        {
            int loc = _gl.GetUniformLocation(Program, name);
            if (loc < 0)
                Logger.Warn("Shader3D", $"Uniform NOT FOUND: {name}");
            else
                Logger.Info("Shader3D", $"Uniform OK: {name} at loc {loc}");
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
                Logger.Error("Shader3D", $"Compile error ({type}): {log}");
                throw new Exception($"3D shader compile error ({type}): {log}");
            }
            Logger.Info("Shader3D", $"{type} compiled OK");
            return s;
        }

        public void Use() => _gl.UseProgram(Program);

        public void SetMatrix4(string name, Matrix4x4 m)
        {
            int loc = _gl.GetUniformLocation(Program, name);
            if (loc < 0) return;

            var t = Matrix4x4.Transpose(m);
            float[] arr =
            {
                t.M11, t.M12, t.M13, t.M14,
                t.M21, t.M22, t.M23, t.M24,
                t.M31, t.M32, t.M33, t.M34,
                t.M41, t.M42, t.M43, t.M44
            };

            unsafe
            {
                fixed (float* p = arr)
                    _gl.UniformMatrix4(loc, 1, false, p);
            }
        }

        public void SetVector3(string name, Vector3 v)
        {
            int loc = _gl.GetUniformLocation(Program, name);
            if (loc < 0) return;
            _gl.Uniform3(loc, v.X, v.Y, v.Z);
        }

        public void SetInt(string name, int value)
        {
            int loc = _gl.GetUniformLocation(Program, name);
            if (loc < 0) return;
            _gl.Uniform1(loc, value);
        }

        public void Dispose()
        {
            if (Program != 0) _gl.DeleteProgram(Program);
            Program = 0;
        }
    }
}