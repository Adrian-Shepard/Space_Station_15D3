#nullable disable
using System;
using System.Numerics;
using Silk.NET.Input;
using SS15.Client.Core;
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

        private GameMap _map;
        private IKeyboard _keyboard;

        public PlayerController(GameMap map, IKeyboard keyboard, Vector2 startPos)
        {
            _map = map;
            _keyboard = keyboard;
            Position = startPos;
        }

        public void Heal(int amount) => Health = Math.Clamp(Health + amount, 0, 100);
        public void TakeDamage(int amount) => Health = Math.Clamp(Health - amount, 0, 100);
        public void Respawn(Vector2 newPos) { Position = newPos; Health = 100; }

        public void Update(float deltaTime)
        {
            float dirX = MathF.Cos(Direction);
            float dirY = MathF.Sin(Direction);

            float speed = Speed * deltaTime;
            float mx = 0, my = 0;

<<<<<<< Updated upstream
            Vector2 newPos = new Vector2(Position.X + movement.X, Position.Y);
            if (_map.IsWalkable(newPos, PlayerHalf))
                Position = new Vector2(newPos.X, Position.Y);

            newPos = new Vector2(Position.X, Position.Y + movement.Y);
            if (_map.IsWalkable(newPos, PlayerHalf))
                Position = new Vector2(Position.X, newPos.Y);
=======
            if (_keyboard.IsKeyPressed(Key.W)) { mx += dirX * speed; my += dirY * speed; }
            if (_keyboard.IsKeyPressed(Key.S)) { mx -= dirX * speed; my -= dirY * speed; }
            if (_keyboard.IsKeyPressed(Key.A)) { mx += dirY * speed; my -= dirX * speed; }
            if (_keyboard.IsKeyPressed(Key.D)) { mx -= dirY * speed; my += dirX * speed; }

            Vector2 newPos = new Vector2(Position.X + mx, Position.Y);
            if (_map.IsWalkable(newPos, PlayerHalf)) Position = new Vector2(newPos.X, Position.Y);

            newPos = new Vector2(Position.X, Position.Y + my);
            if (_map.IsWalkable(newPos, PlayerHalf)) Position = new Vector2(Position.X, newPos.Y);
>>>>>>> Stashed changes
        }
    }
}