using System.Numerics;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Player;
using SS15.Client.Systems;

namespace SS15.Client
{
    public class GameState
    {
        public GameMap Map { get; private set; }
        public PlayerController Player { get; private set; }
        public AtmosphereSystem Atmosphere { get; private set; }
        public PowerSystem Power { get; private set; }
        public HealthSystem Health { get; private set; }

        // Инициализация карты, систем и игрока
        public void Initialize(IKeyboard keyboard)
        {
            Map = MapGenerator.Generate();

            Atmosphere = new AtmosphereSystem();
            Atmosphere.Initialize(Map);

            Power = new PowerSystem();
            Power.Initialize(Map);

            Health = new HealthSystem();

            // Стартовая позиция игрока (центр карты)
            Vector2 startPos = new Vector2((Map.Width / 2) * Map.TileSize + Map.TileSize / 2,
                                           (Map.Height / 2) * Map.TileSize + Map.TileSize / 2);
            Player = new PlayerController(Map, keyboard, startPos);

            // Пример предметов
            Map.AddItemToTile(10, 10, new Item(ItemType.Medkit, "Medkit", 2, new byte[] { 255, 0, 0, 255 }));
            Map.AddItemToTile(20, 15, new Item(ItemType.Food, "Apple", 1, new byte[] { 0, 255, 0, 255 }));
            Map.AddItemToTile(30, 20, new Item(ItemType.Tool, "Wrench", 1, new byte[] { 0, 0, 255, 255 }));
        }

        // Обновление всех систем
        public void Update(float deltaTime)
        {
            Player.Update(deltaTime);
            Atmosphere.Update(deltaTime);
            Power.Update(deltaTime);

            int tileX = (int)(Player.Position.X / Map.TileSize);
            int tileY = (int)(Player.Position.Y / Map.TileSize);
<<<<<<< Updated upstream
            var atmos = Atmosphere.GetAtmosphereAt(tileX, tileY);
            // Передаём карту и позицию игрока для урона от космоса
            Health.Update(Player, atmos, Map, Player.Position, deltaTime);
=======
            AtmosphereData? atmosNullable = Atmosphere.GetAtmosphereAt(tileX, tileY);
            AtmosphereData atmos = atmosNullable ?? default;

            sw.Restart();
            Health.Update(Player, atmos, Map, Player.Position, deltaTime);
            float healthMs = (float)sw.Elapsed.TotalMilliseconds;
>>>>>>> Stashed changes

            // Возрождение при смерти
            if (Player.Health <= 0)
            {
                Player.Respawn(new Vector2((Map.Width / 2) * Map.TileSize + Map.TileSize / 2,
                                           (Map.Height / 2) * Map.TileSize + Map.TileSize / 2));
            }
        }
    }
}