#nullable disable
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Objects.Physics
{
    public class FlyingItem
    {
        public Item Item;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Height;
        public float HeightVelocity;

        // Скольжение после приземления
        public bool Sliding;
        public float SlidingFriction = 3.5f;
    }
}