#nullable disable
using System.Numerics;

namespace SS15.Client.Objects.Physics
{
    public enum PhysicsObjectType
    {
        Crate,
        Barrel,
    }

    public class PhysicsObject
    {
        public PhysicsObjectType Type;
        public Vector2 Position;
        public Vector2 Velocity;

        public float HalfSize;
        public float Height;
        public float Mass;

        // Скорость затухания: 2 = катится долго, 8 = стоп почти мгновенно
        public float Friction;

        // 0 = прилипает, 0.3 = лёгкий отскок, 0.8 = резиновый
        public float Restitution = 0.15f;

        public bool IsStatic;
        public bool IsSleeping;
        public float SleepTimer;

        public PhysicsObject(PhysicsObjectType type, Vector2 pos, float halfSize, float height,
            float mass = 1f, float friction = 4f, float restitution = 0.15f)
        {
            Type = type;
            Position = pos;
            HalfSize = halfSize;
            Height = height;
            Mass = mass;
            Friction = friction;
            Restitution = restitution;
        }
    }
}