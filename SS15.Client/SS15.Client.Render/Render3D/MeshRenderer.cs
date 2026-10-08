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
        private static readonly Vector3 Ambient = new Vector3(0.40f, 0.40f, 0.46f);
        private static readonly Vector3 LightColor = new Vector3(0.75f, 0.72f, 0.65f);

        public MeshRenderer(GL gl, Shader3D shader)
        {
            _gl = gl;
            Shader = shader;
        }

        public void Begin(int viewportWidth, int viewportHeight, Matrix4x4 view, Matrix4x4 proj, Vector3 cameraPos)
        {
            _gl.Viewport(0, 0, (uint)viewportWidth, (uint)viewportHeight);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Less);
            _gl.DepthMask(true);
            _gl.Disable(EnableCap.CullFace);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Shader.Use();

            Shader.SetInt("uTexture", 0);
            Shader.SetInt("uUnlit", 0);
            Shader.SetVector4("uTint", Vector4.One);
            Shader.SetVector3("uLightDir", LightDir);
            Shader.SetVector3("uAmbientColor", Ambient);
            Shader.SetVector3("uLightColor", LightColor);

            Shader.SetMatrix4("uView", view);
            Shader.SetMatrix4("uProjection", proj);
            Shader.SetVector3("uViewPos", cameraPos);
            Shader.SetMatrix4("uRotation", Matrix4x4.Identity);
        }

        public void Draw(Mesh mesh, Texture2D texture, Vector3 pos, Vector3 scale)
        {
            texture.Bind(TextureUnit.Texture0);
            Shader.SetVector3("uModelPos", pos);
            Shader.SetVector3("uModelScale", scale);
            Shader.SetVector4("uTint", Vector4.One);
            Shader.SetMatrix4("uRotation", Matrix4x4.Identity);
            mesh.Draw();
        }

        public void DrawTinted(Mesh mesh, Texture2D texture, Vector3 pos, Vector3 scale, Color color)
        {
            texture.Bind(TextureUnit.Texture0);
            Shader.SetVector3("uModelPos", pos);
            Shader.SetVector3("uModelScale", scale);
            Shader.SetVector4("uTint", new Vector4(color.R, color.G, color.B, color.A));
            Shader.SetMatrix4("uRotation", Matrix4x4.Identity);
            mesh.Draw();
        }

        public void DrawRotated(Mesh mesh, Texture2D texture, Vector3 pos, Vector3 scale, Matrix4x4 rotation)
        {
            texture.Bind(TextureUnit.Texture0);
            Shader.SetVector3("uModelPos", pos);
            Shader.SetVector3("uModelScale", scale);
            Shader.SetVector4("uTint", Vector4.One);
            Shader.SetMatrix4("uRotation", rotation);
            mesh.Draw();
        }

        public void DrawUnlit(Mesh mesh, Texture2D texture, Vector3 pos, Vector3 scale)
        {
            texture.Bind(TextureUnit.Texture0);
            Shader.SetVector3("uModelPos", pos);
            Shader.SetVector3("uModelScale", scale);
            Shader.SetInt("uUnlit", 1);
            Shader.SetVector4("uTint", Vector4.One);
            Shader.SetMatrix4("uRotation", Matrix4x4.Identity);

            _gl.DepthMask(false);
            mesh.Draw();
            _gl.DepthMask(true);

            Shader.SetInt("uUnlit", 0);
        }

        public void End()
        {
            _gl.BindVertexArray(0);
            _gl.UseProgram(0);
        }
    }
}