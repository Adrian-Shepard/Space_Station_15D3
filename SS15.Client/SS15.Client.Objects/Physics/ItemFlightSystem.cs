#nullable disable
using System;
using System.Collections.Generic;
using System.Numerics;
using SS15.Client.Core;

namespace SS15.Client.Objects.Physics
{
    public class ItemFlightSystem
    {
        private readonly List<FlyingItem> _items = new();
        private readonly GameMap _map;

        private const float Gravity = -25f;
        private const float GroundHeight = 0.15f;
        private const float StopSpeedThreshold = 25f;
        private const float AirFriction = 0.6f;
        private const float WallBounce = -0.35f;
        private const float FloorBounce = 0.25f;
        private const float ItemRadius = 6f;
        private const float ThrowEyeHeight = 1.4f;

        private const float MaxUpVelocity = 18f;
        private const float MaxDownVelocity = -22f;
        private const float PitchHeightFactor = 0.20f;

        public IReadOnlyList<FlyingItem> Items => _items;

        public ItemFlightSystem(GameMap map)
        {
            _map = map;
        }

        public void Throw(Item item, Vector2 from, Vector2 horizontalDir, float pitch, float speed,
            float slidingFriction = 3.5f)
        {
            float len = horizontalDir.Length();
            if (len > 0.001f) horizontalDir /= len;

            float cosP = MathF.Cos(pitch);
            float sinP = MathF.Sin(pitch);

            float hv = speed * sinP * PitchHeightFactor;
            if (hv > MaxUpVelocity) hv = MaxUpVelocity;
            if (hv < MaxDownVelocity) hv = MaxDownVelocity;

            // Гарантируем, что даже при взгляде строго вверх есть горизонтальная составляющая
            float horizontal = speed * MathF.Max(0.3f, cosP);

            var fi = new FlyingItem
            {
                Item = item,
                Position = from,
                Velocity = horizontalDir * horizontal,
                Height = ThrowEyeHeight,
                HeightVelocity = hv,
                Sliding = false,
                SlidingFriction = slidingFriction,
            };

            _items.Add(fi);
        }

        public void Update(float dt)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var fi = _items[i];

                Vector2 delta = fi.Velocity * dt;

                if (Math.Abs(delta.X) > 0.001f)
                {
                    Vector2 tryPos = new Vector2(fi.Position.X + delta.X, fi.Position.Y);
                    if (_map.IsWalkable(tryPos, ItemRadius))
                        fi.Position = tryPos;
                    else
                        fi.Velocity.X *= WallBounce;
                }

                if (Math.Abs(delta.Y) > 0.001f)
                {
                    Vector2 tryPos = new Vector2(fi.Position.X, fi.Position.Y + delta.Y);
                    if (_map.IsWalkable(tryPos, ItemRadius))
                        fi.Position = tryPos;
                    else
                        fi.Velocity.Y *= WallBounce;
                }

                if (!fi.Sliding)
                {
                    fi.HeightVelocity += Gravity * dt;
                    fi.Height += fi.HeightVelocity * dt;

                    if (fi.Height <= GroundHeight)
                    {
                        fi.Height = GroundHeight;

                        if (fi.HeightVelocity < -1f)
                        {
                            fi.HeightVelocity *= FloorBounce;
                        }
                        else
                        {
                            fi.HeightVelocity = 0f;
                            fi.Sliding = true;
                        }
                    }
                }

                float friction = fi.Sliding ? fi.SlidingFriction : AirFriction;
                float f = MathF.Max(0f, 1f - friction * dt);
                fi.Velocity *= f;

                float speed = fi.Velocity.Length();

                if (fi.Sliding && speed < StopSpeedThreshold)
                {
                    int tx = (int)(fi.Position.X / _map.TileSize);
                    int ty = (int)(fi.Position.Y / _map.TileSize);
                    _map.AddItemToTile(tx, ty, fi.Item);
                    _items.RemoveAt(i);
                }
            }
        }
    }
}