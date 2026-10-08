using System;
using System.Numerics;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Objects.Physics;
using SS15.Client.Player;
using SS15.Client.Systems;

namespace SS15.Client
{
    public class GameState
    {
        public const float LogicStep = 1f / 20f;
        private const float MaxAccumulator = 0.25f;
        private const float MaxPlayerStep = 0.05f;

        private float _accumulator = 0f;

        public GameMap Map { get; private set; }
        public PlayerController Player { get; private set; }
        public AtmosphereSystem Atmosphere { get; private set; }
        public PowerSystem Power { get; private set; }
        public HealthSystem Health { get; private set; }
        public PhysicsWorld Physics { get; private set; }
        public ItemFlightSystem ItemFlight { get; private set; }

        public PlayerEntityList RemotePlayers { get; } = new();
        public int LocalPlayerId { get; set; } = 0;

        public GameRuntime Runtime { get; } = new();
        public RuntimeErrorLog Errors { get; } = new();

        public int ActiveSlotIndex { get; set; } = 0;
        public int ActiveHandIndex { get; set; } = 0;

        public void Initialize(IKeyboard keyboard)
        {
            Errors.Runtime = Runtime;

            Map = MapGenerator.Generate();

            Atmosphere = new AtmosphereSystem();
            Atmosphere.Initialize(Map);

            Power = new PowerSystem();
            Power.Initialize(Map);

            Health = new HealthSystem();
            Physics = new PhysicsWorld(Map);
            ItemFlight = new ItemFlightSystem(Map);

            Vector2 startPos = new Vector2(
                (Map.Width / 2) * Map.TileSize + Map.TileSize / 2,
                (Map.Height / 2) * Map.TileSize + Map.TileSize / 2);
            Player = new PlayerController(Map, keyboard, startPos);
            Player.SetPhysics(Physics);

            Map.AddItemToTile(10, 10, new Item(ItemType.Medkit, "Medkit", 2, new byte[] { 255, 0, 0, 255 }));
            Map.AddItemToTile(20, 15, new Item(ItemType.Food, "Apple", 1, new byte[] { 0, 255, 0, 255 }));
            Map.AddItemToTile(30, 20, new Item(ItemType.Tool, "Wrench", 1, new byte[] { 0, 0, 255, 255 }));

            SpawnCrates(startPos);

            // Тестовый NPC для проверки отображения модели игрока
            RemotePlayers.Add(new PlayerEntity(1, "TestNPC",
                new Vector2(startPos.X + Map.TileSize * 2f, startPos.Y + Map.TileSize * 2f)));
        }

        private void SpawnCrates(Vector2 center)
        {
            int ts = Map.TileSize;

            Physics.Add(new PhysicsObject(PhysicsObjectType.Crate,
                new Vector2(center.X + ts * 1.5f, center.Y), ts * 0.35f, 0.7f,
                mass: 2f, friction: 4f));

            Physics.Add(new PhysicsObject(PhysicsObjectType.Crate,
                new Vector2(center.X + ts * 1.5f, center.Y + ts * 1.0f), ts * 0.35f, 0.7f,
                mass: 3f, friction: 4f));

            Physics.Add(new PhysicsObject(PhysicsObjectType.Crate,
                new Vector2(center.X - ts * 2.0f, center.Y - ts * 1.5f), ts * 0.35f, 0.7f,
                mass: 1.5f, friction: 4f));
        }

        public (float player, float atmos, float power, float health) Update(float deltaTime)
        {
            Runtime.TickReal(deltaTime);

            float playerDt = Math.Min(deltaTime, MaxPlayerStep);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            SafeCall("Physics.Update", () => Physics.Update(playerDt));
            float physicsMs = (float)sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            SafeCall("ItemFlight.Update", () => ItemFlight.Update(playerDt));
            float itemFlightMs = (float)sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            SafeCall("Player.Update", () => Player.Update(playerDt));
            float playerMs = (float)sw.Elapsed.TotalMilliseconds;

            if (Player.Health <= 0)
            {
                SafeCall("Player.Respawn", () => Player.Respawn(new Vector2(
                    (Map.Width / 2) * Map.TileSize + Map.TileSize / 2,
                    (Map.Height / 2) * Map.TileSize + Map.TileSize / 2)));
            }

            _accumulator += deltaTime;
            if (_accumulator > MaxAccumulator) _accumulator = MaxAccumulator;

            float totalAtmos = 0f, totalPower = 0f, totalHealth = 0f;

            while (_accumulator >= LogicStep)
            {
                Runtime.TickGame(LogicStep);
                var (a, pw, h) = TickWorld(LogicStep);
                totalAtmos += a;
                totalPower += pw;
                totalHealth += h;
                _accumulator -= LogicStep;
            }

            return (playerMs, totalAtmos, totalPower, totalHealth);
        }

        private (float atmos, float power, float health) TickWorld(float dt)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            SafeCall("Atmosphere.Update", () => Atmosphere.Update(dt));
            float atmosMs = (float)sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            SafeCall("Power.Update", () => Power.Update(dt));
            float powerMs = (float)sw.Elapsed.TotalMilliseconds;

            int tileX = (int)(Player.Position.X / Map.TileSize);
            int tileY = (int)(Player.Position.Y / Map.TileSize);

            AtmosphereData? atmosNullable = Atmosphere.GetAtmosphereAt(tileX, tileY);
            AtmosphereData atmos = atmosNullable ?? default;

            sw.Restart();
            SafeCall("Health.Update", () => Health.Update(Player, atmos, Map, Player.Position, dt));
            float healthMs = (float)sw.Elapsed.TotalMilliseconds;

            return (atmosMs, powerMs, healthMs);
        }

        private void SafeCall(string system, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                Errors.Report(system, ex);
                Logger.Error(system, $"Runtime error caught: {ex.Message}", ex);
            }
        }
    }
}