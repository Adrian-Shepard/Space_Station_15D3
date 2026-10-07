#nullable disable
using Silk.NET.OpenGL;
using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.Render
{
    public class MeshRenderer
    {
        private GL _gl;
        public Shader3D Shader { get; }

        private static readonly Vector3 LightDir = new Vector3(-0.3f, -1f, -0.4f);
        private static readonly Vector3 Ambient = new Vector3(0.35f, 0.35f, 0.42f);
        private static readonly Vector3 LightColor = new Vector3(0.85f, 0.82f, 0.75f);

        public MeshRenderer(GL gl, Shader3D shader)
        {
            _gl = gl;
            Shader = shader;
        }

        /// Начало кадра: состояние GL + свет + камера — один раз на кадр.
        public void Begin(int viewportWidth, int viewportHeight, Matrix4x4 view, Matrix4x4 proj, Vector3 cameraPos)
        {
            _gl.Viewport(0, 0, (uint)viewportWidth, (uint)viewportHeight);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Less);
            _gl.DepthMask(true);
            _gl.Disable(EnableCap.CullFace);

            Shader.Use();

            // Постоянные uniform'ы — один раз на кадр
            Shader.SetInt("uTexture", 0);
            Shader.SetVector3("uLightDir", LightDir);
            Shader.SetVector3("uAmbientColor", Ambient);
            Shader.SetVector3("uLightColor", LightColor);

            // Камера — один раз на кадр
            Shader.SetMatrix4("uView", view);
            Shader.SetMatrix4("uProjection", proj);
            Shader.SetVector3("uViewPos", cameraPos);
        }

        /// Отрисовка меша. Только 2 uniform + bind текстуры на объект.
        public void Draw(Mesh mesh, Texture2D texture, Vector3 pos, Vector3 scale)
        {
            texture.Bind(TextureUnit.Texture0);
            Shader.SetVector3("uModelPos", pos);
            Shader.SetVector3("uModelScale", scale);
            mesh.Draw();
        }

        public void End()
        {
            _gl.BindVertexArray(0);
            _gl.UseProgram(0);
        }
    }
}