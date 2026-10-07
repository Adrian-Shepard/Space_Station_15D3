using System;
using System.Collections.Generic;
using SS15.Client.Core;

namespace SS15.Client.Systems
{
    public struct AtmosphereData
    {
        public float Temperature;
        public float MolesOxygen;
        public float MolesNitrogen;
        public float MolesCarbonDioxide;
        public float MolesPlasma;

        public float TotalMoles => MolesOxygen + MolesNitrogen + MolesCarbonDioxide + MolesPlasma;
        public float OxygenPercent => TotalMoles > 0 ? MolesOxygen / TotalMoles * 100f : 0f;
        public float NitrogenPercent => TotalMoles > 0 ? MolesNitrogen / TotalMoles * 100f : 0f;
        public float CarbonDioxidePercent => TotalMoles > 0 ? MolesCarbonDioxide / TotalMoles * 100f : 0f;
        public float PlasmaPercent => TotalMoles > 0 ? MolesPlasma / TotalMoles * 100f : 0f;
        public float Pressure => TotalMoles * 0.003455f * Temperature;
    }

    public class AtmosphereSystem
    {
        private GameMap _map;
        private AtmosphereData[,] _readGrid;
        private AtmosphereData[,] _writeGrid;

        private HashSet<(int x, int y)> _activeTiles = new();
        private List<(int x, int y)> _snapshot = new(4096);

        private float _updateTimer = 0f;
        private const float UpdateInterval = 0.1f;
<<<<<<< Updated upstream
        private const float DiffusionRate = 0.8f;
=======
        private const float DiffusionRate = 0.5f;
        private const float PressureFactor = 0.01f;
        private const float MaxTemperature = 10000f;
        private const float Epsilon = 0.001f;
>>>>>>> Stashed changes

        public void Initialize(GameMap map)
        {
            _map = map;
            int w = map.Width;
            int h = map.Height;
            _readGrid = new AtmosphereData[w, h];
            _writeGrid = new AtmosphereData[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (IsWalkable(x, y))
                    {
                        _readGrid[x, y] = new AtmosphereData
                        {
                            Temperature = 293.15f,
                            MolesOxygen = 21f,
                            MolesNitrogen = 79f,
                            MolesCarbonDioxide = 0.03f,
                            MolesPlasma = 0f
                        };
                        _writeGrid[x, y] = _readGrid[x, y];
                        _activeTiles.Add((x, y));
                    }
                }
            }
        }

        public void Update(float deltaTime)
        {
            _updateTimer += deltaTime;
            if (_updateTimer < UpdateInterval) return;
            _updateTimer = 0f;
<<<<<<< Updated upstream
            DiffuseGases();
        }

        public AtmosphereData GetAtmosphereAt(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= _map.Width || tileY < 0 || tileY >= _map.Height)
                return null;
            return _atmosGrid[tileX, tileY];
        }

        public void ModifyOxygen(int tileX, int tileY, float delta)
        {
            var atmos = GetAtmosphereAt(tileX, tileY);
            if (atmos != null)
                atmos.MolesOxygen = Math.Max(0, atmos.MolesOxygen + delta);
        }

        public void ModifyNitrogen(int tileX, int tileY, float delta)
        {
            var atmos = GetAtmosphereAt(tileX, tileY);
            if (atmos != null)
                atmos.MolesNitrogen = Math.Max(0, atmos.MolesNitrogen + delta);
        }

        public void ModifyCarbonDioxide(int tileX, int tileY, float delta)
        {
            var atmos = GetAtmosphereAt(tileX, tileY);
            if (atmos != null)
                atmos.MolesCarbonDioxide = Math.Max(0, atmos.MolesCarbonDioxide + delta);
        }

        public void ModifyTemperature(int tileX, int tileY, float delta)
        {
            var atmos = GetAtmosphereAt(tileX, tileY);
            if (atmos != null)
                atmos.Temperature = Math.Max(0, atmos.Temperature + delta);
        }

        public void SetWall(int tileX, int tileY, bool wall)
        {
            if (tileX < 0 || tileX >= _map.Width || tileY < 0 || tileY >= _map.Height)
                return;

            _map.SetTile(tileX, tileY, wall ? TileType.Wall : TileType.Floor);
            if (wall)
                _atmosGrid[tileX, tileY] = null;
            else
                if (_atmosGrid[tileX, tileY] == null)
                    _atmosGrid[tileX, tileY] = GetNeighborAverage(tileX, tileY) ?? new AtmosphereData();
        }

        public void SetTileToSpace(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= _map.Width || tileY < 0 || tileY >= _map.Height)
                return;
            _map.SetTile(tileX, tileY, TileType.Space);
            _atmosGrid[tileX, tileY] = null;
        }

        public void SetTileToFloor(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= _map.Width || tileY < 0 || tileY >= _map.Height)
                return;
            _map.SetTile(tileX, tileY, TileType.Floor);
            if (_atmosGrid[tileX, tileY] == null)
                _atmosGrid[tileX, tileY] = GetNeighborAverage(tileX, tileY) ?? new AtmosphereData();
        }

        public void SetDoorState(int tileX, int tileY, bool open)
        {
            if (tileX < 0 || tileX >= _map.Width || tileY < 0 || tileY >= _map.Height)
                return;

            if (open)
=======

            if (_activeTiles.Count == 0) return;

            // Снимок активных тайлов на этот тик
            _snapshot.Clear();
            foreach (var t in _activeTiles) _snapshot.Add(t);
            _activeTiles.Clear();

            // 1. Копируем read -> write для активных и их соседей
            foreach (var (x, y) in _snapshot)
            {
                CopyToWrite(x, y);
                CopyToWrite(x + 1, y);
                CopyToWrite(x - 1, y);
                CopyToWrite(x, y + 1);
                CopyToWrite(x, y - 1);
            }

            // 2. Диффузия
            foreach (var (x, y) in _snapshot)
>>>>>>> Stashed changes
            {
                if (!IsWalkable(x, y)) continue;

                ProcessNeighbor(x, y, x + 1, y);
                ProcessNeighbor(x, y, x - 1, y);
                ProcessNeighbor(x, y, x, y + 1);
                ProcessNeighbor(x, y, x, y - 1);

                // Горение
                ref var cell = ref _writeGrid[x, y];
                if (cell.MolesPlasma > Epsilon && cell.MolesOxygen > Epsilon && cell.Temperature > 373.15f)
                    Burn(ref cell);

                // Изменилось?
                ref var readCell = ref _readGrid[x, y];
                bool changed =
                    Math.Abs(cell.Temperature - readCell.Temperature) > Epsilon ||
                    Math.Abs(cell.MolesOxygen - readCell.MolesOxygen) > Epsilon ||
                    Math.Abs(cell.MolesNitrogen - readCell.MolesNitrogen) > Epsilon ||
                    Math.Abs(cell.MolesCarbonDioxide - readCell.MolesCarbonDioxide) > Epsilon ||
                    Math.Abs(cell.MolesPlasma - readCell.MolesPlasma) > Epsilon;

                if (changed)
                {
                    _activeTiles.Add((x, y));
                    AddNeighborActive(x + 1, y);
                    AddNeighborActive(x - 1, y);
                    AddNeighborActive(x, y + 1);
                    AddNeighborActive(x, y - 1);
                }
            }

            // 3. Своп буферов
            (_readGrid, _writeGrid) = (_writeGrid, _readGrid);
        }

        private void CopyToWrite(int x, int y)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height) return;
            _writeGrid[x, y] = _readGrid[x, y];
        }

        private void ProcessNeighbor(int x1, int y1, int x2, int y2)
        {
            if (x2 < 0 || x2 >= _map.Width || y2 < 0 || y2 >= _map.Height) return;

            if (!IsWalkable(x2, y2))
            {
                // Утечка в космос
                if (_map.GetTile(x2, y2) == TileType.Space)
                {
                    int spaceCount = CountSpaceNeighbors(x1, y1);
                    if (spaceCount > 0)
                    {
                        float leak = MathF.Pow(0.5f, spaceCount); // 50% на соседа
                        ref var aNext = ref _writeGrid[x1, y1];
                        aNext.MolesOxygen *= leak;
                        aNext.MolesNitrogen *= leak;
                        aNext.MolesCarbonDioxide *= leak;
                        aNext.MolesPlasma *= leak;
                        aNext.Temperature = 3f;
                    }
                }
                return;
            }

            ref var aRead = ref _readGrid[x1, y1];
            ref var bRead = ref _readGrid[x2, y2];
            ref var aWrite = ref _writeGrid[x1, y1];
            ref var bWrite = ref _writeGrid[x2, y2];

            float pressureDiff = bRead.Pressure - aRead.Pressure;
            float factor = Math.Clamp(Math.Abs(pressureDiff) * PressureFactor, 0.05f, 1.0f);
            float rate = DiffusionRate * factor;

            float tempDiff = bRead.Temperature - aRead.Temperature;
            float tempTransfer = tempDiff * rate * 0.25f;
            aWrite.Temperature = Math.Clamp(aWrite.Temperature + tempTransfer, 0f, MaxTemperature);
            bWrite.Temperature = Math.Clamp(bWrite.Temperature - tempTransfer, 0f, MaxTemperature);

            Diffuse(ref aRead.MolesOxygen, ref bRead.MolesOxygen, ref aWrite.MolesOxygen, ref bWrite.MolesOxygen, rate);
            Diffuse(ref aRead.MolesNitrogen, ref bRead.MolesNitrogen, ref aWrite.MolesNitrogen, ref bWrite.MolesNitrogen, rate);
            Diffuse(ref aRead.MolesCarbonDioxide, ref bRead.MolesCarbonDioxide, ref aWrite.MolesCarbonDioxide, ref bWrite.MolesCarbonDioxide, rate);
            Diffuse(ref aRead.MolesPlasma, ref bRead.MolesPlasma, ref aWrite.MolesPlasma, ref bWrite.MolesPlasma, rate);
        }

        private void Diffuse(ref float readA, ref float readB, ref float writeA, ref float writeB, float rate)
        {
            float diff = readB - readA;
            float transfer = diff * rate * 0.25f;
            writeA += transfer;
            writeB -= transfer;
        }

        private void Burn(ref AtmosphereData data)
        {
            float oxygenRatio = data.OxygenPercent / 21f;
            float burnRate = 0.05f * oxygenRatio;
            float plasmaBurned = Math.Min(data.MolesPlasma, burnRate);
            float oxygenUsed = plasmaBurned;

            data.MolesPlasma -= plasmaBurned;
            data.MolesOxygen = Math.Max(0, data.MolesOxygen - oxygenUsed);
            data.MolesCarbonDioxide += plasmaBurned;
            data.Temperature = Math.Min(data.Temperature + 20f * plasmaBurned, MaxTemperature);
        }

        private int CountSpaceNeighbors(int x, int y)
        {
            int c = 0;
            if (IsSpace(x + 1, y)) c++;
            if (IsSpace(x - 1, y)) c++;
            if (IsSpace(x, y + 1)) c++;
            if (IsSpace(x, y - 1)) c++;
            return c;
        }

        private bool IsSpace(int x, int y)
        {
            return x >= 0 && x < _map.Width && y >= 0 && y < _map.Height &&
                   _map.GetTile(x, y) == TileType.Space;
        }

        private void AddNeighborActive(int x, int y)
        {
            if (x >= 0 && x < _map.Width && y >= 0 && y < _map.Height && IsWalkable(x, y))
                _activeTiles.Add((x, y));
        }

        private bool IsWalkable(int x, int y)
        {
            var tile = _map.GetTile(x, y);
            return tile != TileType.Wall && tile != TileType.ReinforcedWall && tile != TileType.Space &&
                   (tile != TileType.Door || _map.IsDoorOpen(x, y));
        }

        // ======= Публичные методы =======

        public AtmosphereData? GetAtmosphereAt(int tx, int ty)
        {
<<<<<<< Updated upstream
            var newGrid = new AtmosphereData[_map.Width, _map.Height];
            for (int y = 0; y < _map.Height; y++)
                for (int x = 0; x < _map.Width; x++)
                    if (_atmosGrid[x, y] != null)
                        newGrid[x, y] = new AtmosphereData
                        {
                            Temperature = _atmosGrid[x, y].Temperature,
                            MolesOxygen = _atmosGrid[x, y].MolesOxygen,
                            MolesNitrogen = _atmosGrid[x, y].MolesNitrogen,
                            MolesCarbonDioxide = _atmosGrid[x, y].MolesCarbonDioxide
                        };
=======
            if (tx < 0 || tx >= _map.Width || ty < 0 || ty >= _map.Height) return null;
            if (!IsWalkable(tx, ty)) return null;
            return _readGrid[tx, ty];
        }
>>>>>>> Stashed changes

        public void ModifyOxygen(int x, int y, float d)
        {
            if (!IsWalkable(x, y)) return;
            ref var a = ref _readGrid[x, y];
            a.MolesOxygen = Math.Max(0, a.MolesOxygen + d);
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void ModifyNitrogen(int x, int y, float d)
        {
            if (!IsWalkable(x, y)) return;
            ref var a = ref _readGrid[x, y];
            a.MolesNitrogen = Math.Max(0, a.MolesNitrogen + d);
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void ModifyCarbonDioxide(int x, int y, float d)
        {
            if (!IsWalkable(x, y)) return;
            ref var a = ref _readGrid[x, y];
            a.MolesCarbonDioxide = Math.Max(0, a.MolesCarbonDioxide + d);
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void ModifyPlasma(int x, int y, float d)
        {
            if (!IsWalkable(x, y)) return;
            ref var a = ref _readGrid[x, y];
            a.MolesPlasma = Math.Max(0, a.MolesPlasma + d);
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void ModifyTemperature(int x, int y, float d)
        {
            if (!IsWalkable(x, y)) return;
            ref var a = ref _readGrid[x, y];
            a.Temperature = Math.Clamp(a.Temperature + d, 0f, MaxTemperature);
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void SetWall(int x, int y, bool wall)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height) return;
            _map.SetTile(x, y, wall ? TileType.Wall : TileType.Floor);

            if (wall)
            {
<<<<<<< Updated upstream
                for (int x = 0; x < _map.Width; x++)
                {
                    if (_atmosGrid[x, y] == null) continue;

                    // Утечка в космос (быстрая)
                    TryLeakToSpace(newGrid, x, y, x + 1, y);
                    TryLeakToSpace(newGrid, x, y, x - 1, y);
                    TryLeakToSpace(newGrid, x, y, x, y + 1);
                    TryLeakToSpace(newGrid, x, y, x, y - 1);

                    // Обычный обмен с соседями
                    ExchangeWithNeighbor(newGrid, x, y, x + 1, y);
                    ExchangeWithNeighbor(newGrid, x, y, x - 1, y);
                    ExchangeWithNeighbor(newGrid, x, y, x, y + 1);
                    ExchangeWithNeighbor(newGrid, x, y, x, y - 1);
                }
            }

            _atmosGrid = newGrid;
        }

        private void TryLeakToSpace(AtmosphereData[,] grid, int x1, int y1, int x2, int y2)
        {
            if (x2 < 0 || x2 >= _map.Width || y2 < 0 || y2 >= _map.Height) return;
            if (_map.GetTile(x2, y2) != TileType.Space) return;

            // Мгновенная потеря 95% вещества
            grid[x1, y1].MolesOxygen *= 0.05f;
            grid[x1, y1].MolesNitrogen *= 0.05f;
            grid[x1, y1].MolesCarbonDioxide *= 0.05f;

            // Температура падает до температуры космоса (~3K)
            grid[x1, y1].Temperature = 3f;
        }

        private void ExchangeWithNeighbor(AtmosphereData[,] grid, int x1, int y1, int x2, int y2)
        {
            if (x2 < 0 || x2 >= _map.Width || y2 < 0 || y2 >= _map.Height) return;
            if (grid[x1, y1] == null || grid[x2, y2] == null) return;

            float tempDiff = grid[x2, y2].Temperature - grid[x1, y1].Temperature;
            float tempTransfer = tempDiff * DiffusionRate * 0.5f;
            grid[x1, y1].Temperature += tempTransfer;
            grid[x2, y2].Temperature -= tempTransfer;

            ExchangeMoles(grid, x1, y1, x2, y2, nameof(AtmosphereData.MolesOxygen));
            ExchangeMoles(grid, x1, y1, x2, y2, nameof(AtmosphereData.MolesNitrogen));
            ExchangeMoles(grid, x1, y1, x2, y2, nameof(AtmosphereData.MolesCarbonDioxide));
        }

        private void ExchangeMoles(AtmosphereData[,] grid, int x1, int y1, int x2, int y2, string componentName)
        {
            float a1 = GetMoles(grid[x1, y1], componentName);
            float a2 = GetMoles(grid[x2, y2], componentName);
            float diff = a2 - a1;
            float transfer = diff * DiffusionRate * 0.5f;
            SetMoles(grid[x1, y1], componentName, a1 + transfer);
            SetMoles(grid[x2, y2], componentName, a2 - transfer);
        }

        private float GetMoles(AtmosphereData data, string name)
        {
            return name switch
            {
                nameof(AtmosphereData.MolesOxygen) => data.MolesOxygen,
                nameof(AtmosphereData.MolesNitrogen) => data.MolesNitrogen,
                nameof(AtmosphereData.MolesCarbonDioxide) => data.MolesCarbonDioxide,
                _ => 0f
            };
        }

        private void SetMoles(AtmosphereData data, string name, float value)
        {
            switch (name)
            {
                case nameof(AtmosphereData.MolesOxygen): data.MolesOxygen = Math.Max(0, value); break;
                case nameof(AtmosphereData.MolesNitrogen): data.MolesNitrogen = Math.Max(0, value); break;
                case nameof(AtmosphereData.MolesCarbonDioxide): data.MolesCarbonDioxide = Math.Max(0, value); break;
=======
                _readGrid[x, y] = default;
                _writeGrid[x, y] = default;
            }
            else
            {
                var n = GetNeighborAverage(x, y);
                _readGrid[x, y] = n;
                _writeGrid[x, y] = n;
>>>>>>> Stashed changes
            }
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

<<<<<<< Updated upstream
        private AtmosphereData GetNeighborAverage(int x, int y)
        {
            if (x > 0 && _atmosGrid[x - 1, y] != null) return CloneAtmos(_atmosGrid[x - 1, y]);
            if (x < _map.Width - 1 && _atmosGrid[x + 1, y] != null) return CloneAtmos(_atmosGrid[x + 1, y]);
            if (y > 0 && _atmosGrid[x, y - 1] != null) return CloneAtmos(_atmosGrid[x, y - 1]);
            if (y < _map.Height - 1 && _atmosGrid[x, y + 1] != null) return CloneAtmos(_atmosGrid[x, y + 1]);
            return null;
        }

        private AtmosphereData CloneAtmos(AtmosphereData source)
        {
            return new AtmosphereData
            {
                Temperature = source.Temperature,
                MolesOxygen = source.MolesOxygen,
                MolesNitrogen = source.MolesNitrogen,
                MolesCarbonDioxide = source.MolesCarbonDioxide
            };
=======
        public void SetTileToSpace(int x, int y)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height) return;
            _map.SetTile(x, y, TileType.Space);
            _readGrid[x, y] = default;
            _writeGrid[x, y] = default;
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void SetTileToFloor(int x, int y)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height) return;
            _map.SetTile(x, y, TileType.Floor);
            var n = GetNeighborAverage(x, y);
            if (n.TotalMoles <= 0)
                n = new AtmosphereData { Temperature = 293.15f, MolesOxygen = 21f, MolesNitrogen = 79f, MolesCarbonDioxide = 0.03f };
            _readGrid[x, y] = n;
            _writeGrid[x, y] = n;
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        public void SetDoorState(int x, int y, bool open)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height) return;
            _map.SetDoorOpen(x, y, open);
            if (!open)
            {
                _readGrid[x, y] = default;
                _writeGrid[x, y] = default;
            }
            else
            {
                var n = GetNeighborAverage(x, y);
                if (n.TotalMoles <= 0)
                    n = new AtmosphereData { Temperature = 293.15f, MolesOxygen = 21f, MolesNitrogen = 79f, MolesCarbonDioxide = 0.03f };
                _readGrid[x, y] = n;
                _writeGrid[x, y] = n;
            }
            _activeTiles.Add((x, y));
            AddNeighborActive(x + 1, y); AddNeighborActive(x - 1, y);
            AddNeighborActive(x, y + 1); AddNeighborActive(x, y - 1);
        }

        private AtmosphereData GetNeighborAverage(int x, int y)
        {
            if (x > 0 && IsWalkable(x - 1, y) && _readGrid[x - 1, y].TotalMoles > 0) return _readGrid[x - 1, y];
            if (x < _map.Width - 1 && IsWalkable(x + 1, y) && _readGrid[x + 1, y].TotalMoles > 0) return _readGrid[x + 1, y];
            if (y > 0 && IsWalkable(x, y - 1) && _readGrid[x, y - 1].TotalMoles > 0) return _readGrid[x, y - 1];
            if (y < _map.Height - 1 && IsWalkable(x, y + 1) && _readGrid[x, y + 1].TotalMoles > 0) return _readGrid[x, y + 1];
            return default;
>>>>>>> Stashed changes
        }
    }
}