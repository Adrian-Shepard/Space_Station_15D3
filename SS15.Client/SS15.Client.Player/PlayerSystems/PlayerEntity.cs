#nullable disable
using System.Numerics;

namespace SS15.Client.Player
{
    public class PlayerEntity
    {
        public int Id;
        public string Name;
        public Vector2 Position;
        public float Direction;
        public float Pitch;
        public int Health = 100;
        public PlayerColor Color = PlayerColor.Default;

        public PlayerEntity(int id, string name, Vector2 pos)
        {
            Id = id;
            Name = name;
            Position = pos;
        }
    }

    public struct PlayerColor
    {
        public float R, G, B, A;

        public PlayerColor(float r, float g, float b, float a = 1f)
        {
            R = r; G = g; B = b; A = a;
        }

        public static PlayerColor Default => new PlayerColor(0.85f, 0.55f, 0.55f, 1f);
    }
}