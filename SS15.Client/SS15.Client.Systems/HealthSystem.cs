using System;
using System.Numerics;
using SS15.Client.Core;
using SS15.Client.Player;

namespace SS15.Client.Systems
{
    public class HealthSystem
    {
        private float _updateTimer = 0f;
        private const float UpdateInterval = 1f; // раз в секунду

        public void Update(PlayerController player, AtmosphereData atmos, GameMap map, Vector2 playerPos, float deltaTime)
        {
            _updateTimer += deltaTime;
            if (_updateTimer < UpdateInterval)
                return;
            _updateTimer = 0f;

            // Урон от космоса (вакуума)
            int tileX = (int)(playerPos.X / map.TileSize);
            int tileY = (int)(playerPos.Y / map.TileSize);
            if (map.GetTile(tileX, tileY) == TileType.Space)
            {
                player.TakeDamage(15);
                return;
            }

            // Если атмосфера отсутствует (TotalMoles == 0), но не космос — тоже урон
            if (atmos.TotalMoles <= 0)
            {
                player.TakeDamage(5);
                return;
            }

            // Давление
            if (atmos.Pressure < 50f)
                player.TakeDamage(5);
            else if (atmos.Pressure > 150f)
                player.TakeDamage(10);

            // Температура
            float tempC = atmos.Temperature - 273.15f;
            if (tempC < 0f)
                player.TakeDamage(5);
            else if (tempC > 50f)
                player.TakeDamage(8);

            // Кислород
            if (atmos.OxygenPercent < 16f)
                player.TakeDamage(5);

            // CO2
            if (atmos.CarbonDioxidePercent > 5f)
                player.TakeDamage(3);

            // Восстановление при нормальных условиях
            if (atmos.Pressure >= 50f && atmos.Pressure <= 150f &&
                tempC >= 0f && tempC <= 50f &&
                atmos.OxygenPercent >= 16f &&
                atmos.CarbonDioxidePercent <= 5f)
            {
                player.Heal(1);
            }
        }
    }
}