using System;
using System.Numerics;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Player;
using SS15.Client.Systems;

namespace SS15.Client
{
    public class GameState
    {
        /// Фиксированный шаг симуляции — 20 Hz.
        /// В онлайне: сервер тикает с той же частотой, что даёт детерминизм.
        public const float LogicStep = 1f / 20f;

        /// Защита от "spiral of death" при лагах.
        private const float MaxAccumulator = 0.25f;

        private float _accumulator = 0f;

        public GameMap Map { get; private set; }
        public PlayerController Player { get; private set; }
        public AtmosphereSystem Atmosphere { get; private set; }
        public PowerSystem Power { get; private set; }
        public HealthSystem Health { get; private set; }

        public void Initialize(IKeyboard keyboard)
        {
            Map = MapGenerator.Generate();

            Atmosphere = new AtmosphereSystem();
            Atmosphere.Initialize(Map);

            Power = new PowerSystem();
            Power.Initialize(Map);

            Health = new HealthSystem();

            Vector2 startPos = new Vector2(
                (Map.Width / 2) * Map.TileSize + Map.TileSize / 2,
                (Map.Height / 2) * Map.TileSize + Map.TileSize / 2);
            Player = new PlayerController(Map, keyboard, startPos);

            Map.AddItemToTile(10, 10, new Item(ItemType.Medkit, "Medkit", 2, new byte[] { 255, 0, 0, 255 }));
            Map.AddItemToTile(20, 15, new Item(ItemType.Food, "Apple", 1, new byte[] { 0, 255, 0, 255 }));
            Map.AddItemToTile(30, 20, new Item(ItemType.Tool, "Wrench", 1, new byte[] { 0, 0, 255, 255 }));
        }

        /// Вызывается раз в кадр. Внутри может отработать 0..N тиков логики.
        /// Возвращает суммарное время каждой системы за все тики этого кадра.
        public (float player, float atmos, float power, float health) Update(float deltaTime)
        {
            _accumulator += deltaTime;
            if (_accumulator > MaxAccumulator) _accumulator = MaxAccumulator;

            float totalPlayer = 0f, totalAtmos = 0f, totalPower = 0f, totalHealth = 0f;

            while (_accumulator >= LogicStep)
            {
                var (p, a, pw, h) = Tick(LogicStep);
                totalPlayer += p;
                totalAtmos += a;
                totalPower += pw;
                totalHealth += h;
                _accumulator -= LogicStep;
            }

            return (totalPlayer, totalAtmos, totalPower, totalHealth);
        }

        /// Один тик логики — детерминированный, без привязки к FPS.
        private (float player, float atmos, float power, float health) Tick(float dt)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Player.Update(dt);
            float playerMs = (float)sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            Atmosphere.Update(dt);
            float atmosMs = (float)sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            Power.Update(dt);
            float powerMs = (float)sw.Elapsed.TotalMilliseconds;

            int tileX = (int)(Player.Position.X / Map.TileSize);
            int tileY = (int)(Player.Position.Y / Map.TileSize);

            AtmosphereData? atmosNullable = Atmosphere.GetAtmosphereAt(tileX, tileY);
            AtmosphereData atmos = atmosNullable ?? default;

            sw.Restart();
            Health.Update(Player, atmos, Map, Player.Position, dt);
            float healthMs = (float)sw.Elapsed.TotalMilliseconds;

            if (Player.Health <= 0)
            {
                Player.Respawn(new Vector2(
                    (Map.Width / 2) * Map.TileSize + Map.TileSize / 2,
                    (Map.Height / 2) * Map.TileSize + Map.TileSize / 2));
            }

            return (playerMs, atmosMs, powerMs, healthMs);
        }
    }
}