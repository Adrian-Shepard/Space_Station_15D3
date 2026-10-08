using System;
using System.Numerics;

namespace SS15.Client.Render
{
    public class Camera3D
    {
        public Vector3 Position = Vector3.Zero;
        public float Yaw = 0f;
        public float Pitch = 0f;
        public float Fov = MathF.PI / 2.5f;
        public float NearPlane = 0.05f;
        public float FarPlane = 200f;

        public int ViewportWidth { get; set; } = 800;
        public int ViewportHeight { get; set; } = 600;

        public Vector3 Forward
        {
            get
            {
                float cp = MathF.Cos(Pitch);
                float sp = MathF.Sin(Pitch);
                return new Vector3(
                    cp * MathF.Cos(Yaw),
                    sp,
                    cp * MathF.Sin(Yaw)
                );
            }
        }

        public Matrix4x4 GetViewMatrix()
        {
            // Позиция смотрит на Position + Forward, верх = +Y (мировой)
            return Matrix4x4.CreateLookAt(Position, Position + Forward, Vector3.UnitY);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            float aspect = (float)ViewportWidth / ViewportHeight;
            if (aspect <= 0) aspect = 1f;
            return Matrix4x4.CreatePerspectiveFieldOfView(Fov, aspect, NearPlane, FarPlane);
        }
    }
}