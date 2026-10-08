using System;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;
using SS15.Client.Player;
using SS15.Client.Systems;

namespace SS15.Client.Rendering
{
    public class WorldRenderer
    {
        private SpriteBatch _spriteBatch;
        private Camera2D _camera;
        private GameMap _map;
        private PlayerController _player;
        private PowerSystem _powerSystem;

        private Texture2D _floorTex;
        private Texture2D _spaceTex;
        private Texture2D _wallSingleTex, _wallNorthSouthTex, _wallWestEastTex;
        private Texture2D _wallEastSouthTex, _wallNorthEastTex, _wallSouthWestTex, _wallWestNorthTex;
        private Texture2D _gridTex, _playerTex, _itemTex, _whiteTex, _generatorTex, _lampOnTex, _lampOffTex;
        private Texture2D _doorOpenTex, _doorClosedTex;

        private const int TileSize = 32;
        private const int GridThickness = 1; // тонкая сетка
        private const int WireThickness = 3;

        public WorldRenderer(SpriteBatch spriteBatch, Camera2D camera, GameMap map, PlayerController player, PowerSystem powerSystem)
        {
            _spriteBatch = spriteBatch;
            _camera = camera;
            _map = map;
            _player = player;
            _powerSystem = powerSystem;
        }

        public void SetTextures(
            Texture2D floor,
            Texture2D space,
            Texture2D wallSingle,
            Texture2D wallNorthSouth, Texture2D wallWestEast,
            Texture2D wallEastSouth, Texture2D wallNorthEast, Texture2D wallSouthWest, Texture2D wallWestNorth,
            Texture2D grid, Texture2D player, Texture2D item,
            Texture2D white,
            Texture2D generator, Texture2D lampOn, Texture2D lampOff,
            Texture2D doorOpen, Texture2D doorClosed)
        {
            _floorTex = floor;
            _spaceTex = space;
            _wallSingleTex = wallSingle;
            _wallNorthSouthTex = wallNorthSouth;
            _wallWestEastTex = wallWestEast;
            _wallEastSouthTex = wallEastSouth;
            _wallNorthEastTex = wallNorthEast;
            _wallSouthWestTex = wallSouthWest;
            _wallWestNorthTex = wallWestNorth;
            _gridTex = grid;
            _playerTex = player;
            _itemTex = item;
            _whiteTex = white;
            _generatorTex = generator;
            _lampOnTex = lampOn;
            _lampOffTex = lampOff;
            _doorOpenTex = doorOpen;
            _doorClosedTex = doorClosed;
        }

        public void Draw()
        {
            float zoom = _camera.Zoom;
            // Правильный расчёт видимой области в тайлах
            int visibleWidthTiles = (int)Math.Ceiling(_camera.ViewportWidth / (TileSize * zoom));
            int visibleHeightTiles = (int)Math.Ceiling(_camera.ViewportHeight / (TileSize * zoom));

            int startTileX = Math.Max(0, (int)(_camera.Position.X / TileSize) - 1);
            int startTileY = Math.Max(0, (int)(_camera.Position.Y / TileSize) - 1);
            int endTileX = Math.Min(_map.Width - 1, startTileX + visibleWidthTiles + 1);
            int endTileY = Math.Min(_map.Height - 1, startTileY + visibleHeightTiles + 1);

            // Отрисовка тайлов
            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    TileType tile = _map.GetTile(x, y);
                    if (tile == TileType.Wire) continue;

                    if (tile != TileType.Floor && tile != TileType.Space)
                    {
                        _spriteBatch.Draw(_floorTex,
                            new Vector2(x * TileSize, y * TileSize),
                            null, Vector2.One,
                            0, Vector2.Zero, Color.White);
                    }

                    Texture2D tex;
                    switch (tile)
                    {
                        case TileType.Floor:
                            tex = _floorTex;
                            break;
                        case TileType.Space:
                            tex = _spaceTex;
                            break;
                        case TileType.Wall:
                        case TileType.ReinforcedWall:
                            tex = GetWallTexture(x, y);
                            break;
                        case TileType.Generator:
                            tex = _generatorTex;
                            break;
                        case TileType.Lamp:
                            tex = _powerSystem.IsPowered(x, y) ? _lampOnTex : _lampOffTex;
                            break;
                        case TileType.Door:
                            tex = _map.IsDoorOpen(x, y) ? _doorOpenTex : _doorClosedTex;
                            break;
                        default:
                            tex = _floorTex;
                            break;
                    }

                    _spriteBatch.Draw(tex,
                        new Vector2(x * TileSize, y * TileSize),
                        null, Vector2.One,
                        0, Vector2.Zero, Color.White);
                }
            }

            // Сетка
            for (int x = startTileX; x <= endTileX + 1; x++)
                _spriteBatch.Draw(_gridTex,
                    new Vector2(x * TileSize - GridThickness / 2f, startTileY * TileSize),
                    null, new Vector2(GridThickness, (endTileY - startTileY + 1) * TileSize),
                    0, Vector2.Zero, Color.White);
            for (int y = startTileY; y <= endTileY + 1; y++)
                _spriteBatch.Draw(_gridTex,
                    new Vector2(startTileX * TileSize, y * TileSize - GridThickness / 2f),
                    null, new Vector2((endTileX - startTileX + 1) * TileSize, GridThickness),
                    0, Vector2.Zero, Color.White);

            // Провода
            Color wireColor = new Color(1f, 0.8f, 0f, 1f);
            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    if (_map.GetTile(x, y) != TileType.Wire) continue;
                    Vector2 center = new Vector2(x * TileSize + TileSize / 2f, y * TileSize + TileSize / 2f);
                    if (IsConductive(x, y - 1))
                        DrawWireLine(center, new Vector2(center.X, center.Y - TileSize), WireThickness, wireColor);
                    if (IsConductive(x, y + 1))
                        DrawWireLine(center, new Vector2(center.X, center.Y + TileSize), WireThickness, wireColor);
                    if (IsConductive(x - 1, y))
                        DrawWireLine(center, new Vector2(center.X - TileSize, center.Y), WireThickness, wireColor);
                    if (IsConductive(x + 1, y))
                        DrawWireLine(center, new Vector2(center.X + TileSize, center.Y), WireThickness, wireColor);
                    _spriteBatch.Draw(_whiteTex, center - new Vector2(WireThickness / 2f, WireThickness / 2f), null,
                        new Vector2(WireThickness, WireThickness), 0, Vector2.Zero, wireColor);
                }
            }

            // Предметы
            foreach (var kvp in _map.ItemsOnGround)
            {
                int tileX = kvp.Key.x;
                int tileY = kvp.Key.y;
                if (tileX < startTileX || tileX > endTileX || tileY < startTileY || tileY > endTileY)
                    continue;
                foreach (var item in kvp.Value)
                {
                    var pos = new Vector2(tileX * TileSize + TileSize / 2, tileY * TileSize + TileSize / 2);
                    _spriteBatch.Draw(_itemTex, pos - new Vector2(8, 8), null, new Vector2(16, 16), 0, Vector2.Zero, Color.White);
                }
            }

            // Игрок
            _spriteBatch.Draw(_playerTex,
                _player.Position - new Vector2(TileSize / 2f, TileSize / 2f),
                null, Vector2.One, 0, Vector2.Zero, Color.White);
        }

        private bool IsWall(int x, int y)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height)
                return false;
            var tile = _map.GetTile(x, y);
            return tile == TileType.Wall || tile == TileType.ReinforcedWall || tile == TileType.Door;
        }

        private Texture2D GetWallTexture(int x, int y)
        {
            bool north = IsWall(x, y - 1);
            bool south = IsWall(x, y + 1);
            bool west = IsWall(x - 1, y);
            bool east = IsWall(x + 1, y);
            if (!north && !south && !west && !east) return _wallSingleTex;
            if (north && south && !west && !east) return _wallNorthSouthTex;
            if (west && east && !north && !south) return _wallWestEastTex;
            if (south && east && !north && !west) return _wallEastSouthTex;
            if (north && east && !south && !west) return _wallNorthEastTex;
            if (south && west && !north && !east) return _wallSouthWestTex;
            if (west && north && !south && !east) return _wallWestNorthTex;
            return _wallSingleTex;
        }

        private bool IsConductive(int x, int y)
        {
            if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height)
                return false;
            var tile = _map.GetTile(x, y);
            return tile == TileType.Wire || tile == TileType.Generator || tile == TileType.Lamp;
        }

        private void DrawWireLine(Vector2 from, Vector2 to, float thickness, Color color)
        {
            Vector2 diff = to - from;
            float length = diff.Length();
            if (MathF.Abs(diff.X) > 0.01f)
            {
                float x = Math.Min(from.X, to.X);
                float y = from.Y - thickness / 2f;
                _spriteBatch.Draw(_whiteTex, new Vector2(x, y), null, new Vector2(length, thickness), 0, Vector2.Zero, color);
            }
            else if (MathF.Abs(diff.Y) > 0.01f)
            {
                float x = from.X - thickness / 2f;
                float y = Math.Min(from.Y, to.Y);
                _spriteBatch.Draw(_whiteTex, new Vector2(x, y), null, new Vector2(thickness, length), 0, Vector2.Zero, color);
            }
        }
    }
}