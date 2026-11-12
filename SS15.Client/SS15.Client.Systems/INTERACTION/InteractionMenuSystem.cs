#nullable disable
using System;
using Engine.Rendering;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.NTUI;

namespace SS15.Client.Systems
{
    public class InteractionMenuSystem
    {
        private readonly GameContext _ctx;
        private readonly WorldInteractionSystem _world;
        private bool _wasAltDown;
        private bool _lmbInMenuPrev;

        public InteractionMenuSystem(GameContext ctx, WorldInteractionSystem world)
        {
            _ctx = ctx;
            _world = world;
        }

        public void Update(float deltaTime)
        {
            bool altDown = _ctx.Keyboard.IsKeyPressed(Key.AltLeft) || _ctx.Keyboard.IsKeyPressed(Key.AltRight);
            bool rmbDown = _ctx.Mouse.IsButtonPressed(MouseButton.Right);

            if (altDown && !_wasAltDown)
            {
                SetCursorMode(CursorMode.Normal);
                _ctx.Input.ResetMouseTracking();
            }

            if (!altDown && _wasAltDown)
            {
                SetCursorMode(CursorMode.Raw);
                _ctx.Input.ResetMouseTracking();
            }

            if (altDown && rmbDown && !_ctx.Menu.IsOpen)
                BuildMenu();

            if (_ctx.Menu.IsOpen)
            {
                _ctx.Menu.HoveredIndex = InteractionMenuRenderer.HitTest(
                    _ctx.Menu, _ctx.Mouse.Position.X, _ctx.Mouse.Position.Y,
                    _ctx.ScreenWidth, _ctx.ScreenHeight);

                bool lmb = _ctx.Mouse.IsButtonPressed(MouseButton.Left);
                if (lmb && !_lmbInMenuPrev)
                {
                    var hovered = _ctx.Menu.GetHovered();
                    if (hovered != null)
                    {
                        try { hovered.Execute(); }
                        catch (Exception ex)
                        {
                            _ctx.State.Errors.Report("InteractionMenu", ex);
                            Logger.Error("InteractionMenu", $"Action failed: {ex.Message}", ex);
                        }
                    }
                    _ctx.Menu.Clear();
                }
                _lmbInMenuPrev = lmb;
            }
            else
            {
                _lmbInMenuPrev = false;
            }

            if (!altDown && _ctx.Menu.IsOpen)
                _ctx.Menu.Clear();

            _ctx.Input.Blocked = altDown;
            _wasAltDown = altDown;
        }

        private void BuildMenu()
        {
            _ctx.Menu.Clear();
            if (!_ctx.CurrentHit.Hit) return;

            int x = _ctx.CurrentHit.X;
            int y = _ctx.CurrentHit.Z;
            var tile = _ctx.State.Map.GetTileType(x, y);

            _ctx.Menu.Title = $"{GetTileName(tile)} ({x},{y})";

            switch (tile)
            {
                case TileType.Door:
                    _ctx.Menu.Add("Открыть / Закрыть", () => _world.ToggleDoor(x, y));
                    _ctx.Menu.Add("Удалить", () => _world.SafeCall("MenuRemove", () => _world.RemoveObject(x, y)));
                    break;

                case TileType.Wall:
                case TileType.ReinforcedWall:
                    _ctx.Menu.Add("Удалить стену", () => _world.SafeCall("MenuRemove", () => _world.RemoveObject(x, y)));
                    break;

                case TileType.Floor:
                    _ctx.Menu.Add("Построить стену",     () => _world.SafeCall("MenuWall",   () => _world.PlaceWall(x, y)));
                    _ctx.Menu.Add("Построить дверь",     () => _world.SafeCall("MenuDoor",   () => _world.PlaceDoor(x, y)));
                    _ctx.Menu.Add("Проложить провод",    () => _world.SafeCall("MenuWire",   () => _world.PlaceWire(x, y)));
                    _ctx.Menu.Add("Поставить генератор", () => _world.SafeCall("MenuGen",    () => _world.PlaceGenerator(x, y)));
                    _ctx.Menu.Add("Поставить лампу",     () => _world.SafeCall("MenuLamp",   () => _world.PlaceLamp(x, y)));
                    _ctx.Menu.Add("Убрать в космос",     () => _world.SafeCall("MenuRemove", () => _world.RemoveObject(x, y)));
                    break;

                case TileType.Space:
                    _ctx.Menu.Add("Восстановить пол", () => _world.SafeCall("MenuRestore", () => _world.RemoveObject(x, y)));
                    break;

                case TileType.Wire:
                case TileType.Lamp:
                case TileType.Generator:
                    _ctx.Menu.Add("Удалить", () => _world.SafeCall("MenuRemove", () => _world.RemoveObject(x, y)));
                    break;
            }

            var items = _ctx.State.Map.GetItemsAtTile(x, y);
            if (items != null && items.Count > 0)
            {
                string label = items.Count == 1
                    ? $"Подобрать: {items[0].Name}"
                    : $"Подобрать (первый: {items[0].Name})";
                _ctx.Menu.Add(label, () => _world.PickupItem(x, y));
            }
        }

        private void SetCursorMode(CursorMode mode)
        {
            try { _ctx.Mouse.Cursor.CursorMode = mode; } catch { }
        }

        private static string GetTileName(TileType tile) => tile switch
        {
            TileType.Floor          => "Floor",
            TileType.Wall           => "Wall",
            TileType.ReinforcedWall => "Reinforced Wall",
            TileType.Wire           => "Wire",
            TileType.Generator      => "Generator",
            TileType.Lamp           => "Lamp",
            TileType.Door           => "Door",
            TileType.Space          => "Space",
            _                       => tile.ToString()
        };
    }
}