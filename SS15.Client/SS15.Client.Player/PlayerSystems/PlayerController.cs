#nullable disable
using System;
using System.Numerics;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Objects.Physics;

namespace SS15.Client.Player
{
    public class PlayerController
    {
        public Vector2 Position { get; set; }
        public float Speed { get; set; } = 150f;
        public float PlayerHalf { get; } = 12f;
        public int Health { get; private set; } = 100;
        public Inventory Inventory { get; } = new();

        public float Direction { get; set; } = 0f;
        public float Pitch { get; set; } = 0f;

        public float Momentum { get; private set; } = 0f;
        public float MaxBhopBonus { get; set; } = 4.0f;
        public float MomentumPerJump { get; set; } = 0.12f;
        public float MomentumDecay { get; set; } = 0.8f;
        public float JumpGraceTime { get; set; } = 0.35f;

        public float SprintMultiplier { get; set; } = 1.5f;
        public bool IsSprinting { get; private set; }

        public float CurrentSpeedMultiplier => 1f + Momentum * MaxBhopBonus;

        public float PushForce { get; set; } = 900f;

        private const float MaxMoveStep = 8f;

        private GameMap _map;
        private IKeyboard _keyboard;
        private PhysicsWorld _physics;

        private bool _spacePrev;
        private float _sinceLastJump;
        private float _lastDt;

        public PlayerController(GameMap map, IKeyboard keyboard, Vector2 startPos)
        {
            _map = map;
            _keyboard = keyboard;
            Position = startPos;
        }

        public void SetPhysics(PhysicsWorld physics)
        {
            _physics = physics;
        }

        public void Heal(int amount) => Health = Math.Clamp(Health + amount, 0, 100);
        public void TakeDamage(int amount) => Health = Math.Clamp(Health - amount, 0, 100);

        public void SetHealth(int hp)
        {
            Health = Math.Clamp(hp, 0, 100);
        }

        public void Respawn(Vector2 newPos)
        {
            Position = newPos;
            Health = 100;
            Momentum = 0f;
        }

        public void Update(float deltaTime)
        {
            _lastDt = deltaTime;

            bool spaceNow = _keyboard.IsKeyPressed(Key.Space);
            if (spaceNow && !_spacePrev)
            {
                Momentum = Math.Min(1f, Momentum + MomentumPerJump);
                _sinceLastJump = 0f;
            }
            else
            {
                _sinceLastJump += deltaTime;
                if (_sinceLastJump > JumpGraceTime)
                    Momentum = Math.Max(0f, Momentum - MomentumDecay * deltaTime);
            }
            _spacePrev = spaceNow;

            IsSprinting = _keyboard.IsKeyPressed(Key.ShiftLeft) || _keyboard.IsKeyPressed(Key.ShiftRight);

            float mult = CurrentSpeedMultiplier;
            if (IsSprinting) mult *= SprintMultiplier;

            float speed = Speed * mult * deltaTime;

            float dirX = MathF.Cos(Direction);
            float dirY = MathF.Sin(Direction);

            float mx = 0, my = 0;
            if (_keyboard.IsKeyPressed(Key.W)) { mx += dirX * speed; my += dirY * speed; }
            if (_keyboard.IsKeyPressed(Key.S)) { mx -= dirX * speed; my -= dirY * speed; }
            if (_keyboard.IsKeyPressed(Key.A)) { mx += dirY * speed; my -= dirX * speed; }
            if (_keyboard.IsKeyPressed(Key.D)) { mx -= dirY * speed; my += dirX * speed; }

            float dist = MathF.Sqrt(mx * mx + my * my);
            if (dist < 0.001f) return;

            int steps = Math.Max(1, (int)MathF.Ceiling(dist / MaxMoveStep));
            float stepX = mx / steps;
            float stepY = my / steps;

            for (int i = 0; i < steps; i++)
            {
                TryStep(stepX, 0f);
                TryStep(0f, stepY);
            }
        }

        private void TryStep(float dx, float dy)
        {
            Vector2 newPos = new Vector2(Position.X + dx, Position.Y + dy);

            if (!_map.IsWalkable(newPos, PlayerHalf)) return;

            if (_physics != null)
            {
                var blocker = _physics.FindBlocking(newPos, PlayerHalf);
                if (blocker != null)
                {
                    if (blocker.IsStatic) return;

                    float len = MathF.Sqrt(dx * dx + dy * dy);
                    if (len > 0.001f && _lastDt > 0f)
                    {
                        Vector2 dir = new Vector2(dx / len, dy / len);
                        float impulseMag = PushForce * _lastDt;
                        _physics.ApplyImpulse(blocker, dir * impulseMag);
                    }
                    return;
                }
            }

            Position = newPos;
        }
    }
}