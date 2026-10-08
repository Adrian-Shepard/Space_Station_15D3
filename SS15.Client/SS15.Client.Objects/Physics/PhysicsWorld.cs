#nullable disable
using System;
using System.Collections.Generic;
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Objects.Physics
{
    public class PhysicsWorld
    {
        private readonly List<PhysicsObject> _objects = new();
        private readonly GameMap _map;

        private const float SleepThreshold = 3f;
        private const float SleepDelay = 0.4f;
        private const float MaxSubStep = 4f;
        private const float MaxSpeed = 1500f;

        public IReadOnlyList<PhysicsObject> Objects => _objects;

        public PhysicsWorld(GameMap map)
        {
            _map = map;
        }

        public void Add(PhysicsObject obj) => _objects.Add(obj);
        public void Remove(PhysicsObject obj) => _objects.Remove(obj);

        public void Update(float dt)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                if (obj.IsStatic) continue;

                float speed = obj.Velocity.Length();

                if (speed < SleepThreshold)
                {
                    obj.Velocity = Vector2.Zero;
                    obj.SleepTimer += dt;
                    if (obj.SleepTimer >= SleepDelay)
                        obj.IsSleeping = true;
                    continue;
                }

                obj.SleepTimer = 0f;
                obj.IsSleeping = false;

                if (speed > MaxSpeed)
                {
                    obj.Velocity *= MaxSpeed / speed;
                }

                Vector2 fullDelta = obj.Velocity * dt;
                float dist = fullDelta.Length();
                int steps = Math.Max(1, (int)MathF.Ceiling(dist / MaxSubStep));
                float stepDt = dt / steps;

                for (int s = 0; s < steps; s++)
                {
                    if (obj.Velocity.LengthSquared() < 0.01f) break;

                    bool collided = Step(obj, stepDt);
                    if (collided) break;
                }

                float f = MathF.Max(0f, 1f - obj.Friction * dt);
                obj.Velocity *= f;
            }
        }

        private bool Step(PhysicsObject obj, float stepDt)
        {
            bool collided = false;
            Vector2 delta = obj.Velocity * stepDt;

            if (Math.Abs(delta.X) > 0.001f)
            {
                Vector2 tryPos = new Vector2(obj.Position.X + delta.X, obj.Position.Y);

                if (!_map.IsWalkable(tryPos, obj.HalfSize))
                {
                    obj.Velocity.X = 0f;
                    collided = true;
                }
                else
                {
                    var blocker = FindBlocking(obj, tryPos, obj.HalfSize);
                    if (blocker == null)
                    {
                        obj.Position = tryPos;
                    }
                    else
                    {
                        Vector2 n = new Vector2(MathF.Sign(delta.X), 0f);
                        ResolveCollision(obj, blocker, n);
                        collided = true;
                    }
                }
            }

            if (!collided && Math.Abs(delta.Y) > 0.001f)
            {
                Vector2 tryPos = new Vector2(obj.Position.X, obj.Position.Y + delta.Y);

                if (!_map.IsWalkable(tryPos, obj.HalfSize))
                {
                    obj.Velocity.Y = 0f;
                    collided = true;
                }
                else
                {
                    var blocker = FindBlocking(obj, tryPos, obj.HalfSize);
                    if (blocker == null)
                    {
                        obj.Position = tryPos;
                    }
                    else
                    {
                        Vector2 n = new Vector2(0f, MathF.Sign(delta.Y));
                        ResolveCollision(obj, blocker, n);
                        collided = true;
                    }
                }
            }

            return collided;
        }

        private void ResolveCollision(PhysicsObject a, PhysicsObject b, Vector2 normal)
        {
            if (b.IsStatic)
            {
                float vnStatic = a.Velocity.X * normal.X + a.Velocity.Y * normal.Y;
                if (vnStatic > 0f)
                {
                    float eStatic = a.Restitution;
                    Vector2 vNorm = normal * vnStatic;
                    Vector2 vTang = a.Velocity - vNorm;
                    a.Velocity = vTang - vNorm * eStatic;
                }
                return;
            }

            WakeUp(b);

            Vector2 relVel = a.Velocity - b.Velocity;
            float vn = relVel.X * normal.X + relVel.Y * normal.Y;

            if (vn <= 0f) return;

            float e = MathF.Min(a.Restitution, b.Restitution);
            float invMa = a.IsStatic ? 0f : 1f / a.Mass;
            float invMb = 1f / b.Mass;
            float sumInv = invMa + invMb;
            if (sumInv < 1e-6f) return;

            float j = (1f + e) * vn / sumInv;

            if (!a.IsStatic)
                a.Velocity -= normal * (j * invMa);

            b.Velocity += normal * (j * invMb);

            // Защита от взрыва: ограничиваем прирост скорости
            if (b.Velocity.Length() > MaxSpeed)
                b.Velocity *= MaxSpeed / b.Velocity.Length();

            if (!a.IsStatic && a.Velocity.Length() > MaxSpeed)
                a.Velocity *= MaxSpeed / a.Velocity.Length();
        }

        private void WakeUp(PhysicsObject obj)
        {
            obj.IsSleeping = false;
            obj.SleepTimer = 0f;
        }

        public bool CanBeAt(PhysicsObject obj, Vector2 pos)
        {
            if (!_map.IsWalkable(pos, obj.HalfSize)) return false;

            for (int i = 0; i < _objects.Count; i++)
            {
                var other = _objects[i];
                if (other == obj) continue;
                if (AabbOverlap(pos, obj.HalfSize, other.Position, other.HalfSize)) return false;
            }
            return true;
        }

        public void ApplyImpulse(PhysicsObject obj, Vector2 impulse)
        {
            if (obj.IsStatic) return;
            obj.Velocity += impulse / obj.Mass;
            if (obj.Velocity.Length() > MaxSpeed)
                obj.Velocity *= MaxSpeed / obj.Velocity.Length();
            WakeUp(obj);
        }

        public PhysicsObject FindNear(Vector2 pos, float radius)
        {
            PhysicsObject best = null;
            float bestD2 = radius * radius;

            foreach (var obj in _objects)
            {
                float dx = obj.Position.X - pos.X;
                float dy = obj.Position.Y - pos.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < bestD2)
                {
                    best = obj;
                    bestD2 = d2;
                }
            }
            return best;
        }

        public PhysicsObject FindBlocking(PhysicsObject self, Vector2 pos, float half)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                if (obj == self) continue;
                if (AabbOverlap(pos, half, obj.Position, obj.HalfSize))
                    return obj;
            }
            return null;
        }

        public PhysicsObject FindBlocking(Vector2 pos, float half)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                if (AabbOverlap(pos, half, obj.Position, obj.HalfSize))
                    return obj;
            }
            return null;
        }

        private static bool AabbOverlap(Vector2 a, float ha, Vector2 b, float hb)
        {
            return Math.Abs(a.X - b.X) < ha + hb
                && Math.Abs(a.Y - b.Y) < ha + hb;
        }
    }
}