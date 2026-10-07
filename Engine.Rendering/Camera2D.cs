using System.Numerics;

namespace Engine.Rendering
{
    public class Camera2D
    {
        public Vector2 Position;
        public float Zoom = 1f;
        public float IsoYFactor = 1f;
        public float TileHeight = 0f;

        public int ViewportWidth { get; set; } = 800;
        public int ViewportHeight { get; set; } = 600;

        public Matrix4x4 GetViewMatrix() => Matrix4x4.Identity;

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreateOrthographicOffCenter(0, ViewportWidth, ViewportHeight, 0, -1, 1);
        }
    }
}