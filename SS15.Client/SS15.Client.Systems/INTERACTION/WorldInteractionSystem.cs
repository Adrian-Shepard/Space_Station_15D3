#nullable disable
using System;
using System.Numerics;
using Engine.Rendering;
using Silk.NET.Input;
using SS15.Client.Commands;
using SS15.Client.Core;
using SS15.Client.Objects.Physics;

namespace SS15.Client.Systems
{
    public class WorldInteractionSystem
    {
        private readonly GameContext _ctx;
        private const int DoorWidth = 2;

        private PhysicsObject _dragged;
        private bool _fPrev;
        private float _lastDt;

        private const float DragSpeed = 240f;
        private const float DragDistance = 40f;
        private const float DropRange = 96f;
        private const float GrabRange = 64f;

        public PhysicsObject DraggedObject => _dragged;

        public WorldInteractionSystem(GameContext ctx)
        {
            _ctx = ctx;
        }

        public void SetDeltaTime(float dt) => _lastDt = dt;

        public void UpdateInput()
        {
            bool f = _ctx.Keyboard.IsKeyPressed(Key.F);
            if (f && !_fPrev) ToggleGrab();
            _fPrev = f;
            UpdateDrag();
        }

        public void ToggleGrab()
        {
            if (_dragged != null)
            {
                _dragged = null;
                Notify("Ящик отпущен", new Color(0.8f, 0.8f, 0.8f, 1f));
                return;
            }

            var near = _ctx.State.Physics.FindNear(_ctx.State.Player.Position, GrabRange);
            if (near == null)
            {
                Notify("Рядом нет объекта", new Color(1f, 0.85f, 0.5f, 1f));
                return;
            }

            _dragged = near;
            Notify("Взял ящик", new Color(0.7f, 0.9f, 1f, 1f));
        }

        private void UpdateDrag()
        {
            if (_dragged == null) return;
            if (_dragged.IsStatic) { _dragged = null; return; }

            var p = _ctx.State.Player;
            float dirX = MathF.Cos(p.Direction);
            float dirY = MathF.Sin(p.Direction);

            Vector2 target = p.Position + new Vector2(dirX, dirY) * DragDistance;
            Vector2 diff = target - _dragged.Position;
            float dist = diff.Length();

            if (dist > DropRange)
            {
                _dragged = null;
                Notify("Ящик далеко — отпущен", new Color(1f, 0.7f, 0.5f, 1f));
                return;
            }

            if (dist < 1.5f)
            {
                _dragged.Velocity *= 0.85f;
                return;
            }

            Vector2 dir = diff / dist;
            float desired = MathF.Min(dist * 7f, DragSpeed);
            Vector2 desiredVel = dir * desired;
            float k = MathF.Min(1f, _lastDt * 14f);
            _dragged.Velocity = Vector2.Lerp(_dragged.Velocity, desiredVel, k);
        }

        public void LeftClick(int x, int y)
        {
            if (_ctx.State.Map.GetTileType(x, y) == TileType.Door)
                ToggleDoor(x, y);
            else
                PickupItem(x, y);
        }

        public void ToggleDoor(int x, int y)
        {
            var cmd = new ToggleDoorCommand { X = x, Z = y };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success)
                Notify(result.Message, new Color(1f, 0.85f, 0.4f, 1f));
            else
                Notify(result.Message, new Color(1f, 0.5f, 0.5f, 1f));
        }

        public void PickupItem(int x, int y)
        {
            var cmd = new PickupItemCommand { X = x, Z = y };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success)
                Notify(result.Message, new Color(0.6f, 0.9f, 1f, 1f));
            else if (!string.IsNullOrEmpty(result.Message))
                Notify(result.Message, new Color(1f, 0.5f, 0.5f, 1f));
        }

        public void PlaceWall(int x, int y)
        {
            var cmd = new PlaceTileCommand { X = x, Z = y, NewTileType = TileType.Wall };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success) Notify($"Стена ({x},{y})");
        }

        public void PlaceWire(int x, int y)
        {
            var cmd = new PlaceTileCommand { X = x, Z = y, NewTileType = TileType.Wire };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success) Notify($"Провод ({x},{y})");
        }

        public void PlaceGenerator(int x, int y)
        {
            var cmd = new PlaceTileCommand { X = x, Z = y, NewTileType = TileType.Generator };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success) Notify($"Генератор ({x},{y})", new Color(0.9f, 0.8f, 0.4f, 1f));
        }

        public void PlaceLamp(int x, int y)
        {
            var cmd = new PlaceTileCommand { X = x, Z = y, NewTileType = TileType.Lamp };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success) Notify($"Лампа ({x},{y})", new Color(0.9f, 0.9f, 0.5f, 1f));
        }

        public void PlaceDoor(int x, int y)
        {
            float dirX = MathF.Cos(_ctx.State.Player.Direction);
            float dirY = MathF.Sin(_ctx.State.Player.Direction);
            bool facingEastWest = MathF.Abs(dirX) > MathF.Abs(dirY);

            for (int i = 0; i < DoorWidth; i++)
            {
                int tx = facingEastWest ? x : x + i;
                int ty = facingEastWest ? y + i : y;
                if (!CanPlaceDoorTile(tx, ty))
                {
                    Notify($"Нельзя поставить дверь ({tx},{ty})", new Color(1f, 0.5f, 0.5f, 1f));
                    return;
                }
            }

            int gid = _ctx.State.Map.NewDoorGroup();
            for (int i = 0; i < DoorWidth; i++)
            {
                int tx = facingEastWest ? x : x + i;
                int ty = facingEastWest ? y + i : y;

                var cmd = new PlaceTileCommand { X = tx, Z = ty, NewTileType = TileType.Door };
                var result = _ctx.Commands.Dispatch(cmd);
                if (!result.Success) continue;

                _ctx.State.Map.RegisterDoorTile(tx, ty, gid);
                _ctx.State.Atmosphere.SetDoorState(tx, ty, false);
            }

            Notify($"Дверь установлена ({x},{y})", new Color(0.6f, 1f, 0.6f, 1f));
        }

        public void RemoveObject(int x, int y)
        {
            var cmd = new RemoveTileCommand { X = x, Z = y };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success && !string.IsNullOrEmpty(result.Message))
                Notify($"{result.Message} ({x},{y})", new Color(1f, 0.85f, 0.6f, 1f));
        }

        public void DropItem()
        {
            bool ctrl = _ctx.Keyboard.IsKeyPressed(Key.ControlLeft) || _ctx.Keyboard.IsKeyPressed(Key.ControlRight);
            var cmd = new DropItemCommand { Throw = ctrl };
            var result = _ctx.Commands.Dispatch(cmd);
            if (result.Success && !string.IsNullOrEmpty(result.Message))
                Notify(result.Message, new Color(1f, 0.9f, 0.4f, 1f));
            else if (!string.IsNullOrEmpty(result.Message))
                Notify(result.Message, new Color(1f, 0.5f, 0.5f, 1f));
        }

        public void PlaceInPocket()
        {
            int h = _ctx.State.ActiveHandIndex;
            var item = _ctx.State.Player.Inventory.RemoveItem(h);
            if (item != null)
            {
                if (!_ctx.State.Player.Inventory.AddToPocket(item) &&
                    !_ctx.State.Player.Inventory.AddToBelt(item) &&
                    !_ctx.State.Player.Inventory.AddToBack(item))
                    _ctx.State.Player.Inventory.SetItem(h, item);
                else
                    Notify($"В карман: {item.Name}");
            }
        }

        public void UseItem()
        {
            int h = _ctx.State.ActiveHandIndex;
            var item = _ctx.State.Player.Inventory.GetItem(h);
            if (item != null && _ctx.State.Player.Inventory.UseItem(h, _ctx.State.Player))
                Notify($"Использовано: {item.Name}", new Color(0.7f, 1f, 0.7f, 1f));
        }

        public void ModifyOxygen(int x, int y, float d) => DispatchAtmos(x, y, AtmosField.Oxygen, d);
        public void ModifyNitrogen(int x, int y, float d) => DispatchAtmos(x, y, AtmosField.Nitrogen, d);
        public void ModifyCarbonDioxide(int x, int y, float d) => DispatchAtmos(x, y, AtmosField.CarbonDioxide, d);
        public void ModifyTemperature(int x, int y, float d) => DispatchAtmos(x, y, AtmosField.Temperature, d);
        public void ModifyPlasma(int x, int y, float d) => DispatchAtmos(x, y, AtmosField.Plasma, d);

        private void DispatchAtmos(int x, int y, AtmosField field, float delta)
        {
            var cmd = new ModifyAtmosphereCommand { X = x, Z = y, Field = field, Delta = delta };
            _ctx.Commands.Dispatch(cmd);
        }

        private bool CanPlaceDoorTile(int x, int y)
        {
            if (x < 0 || x >= _ctx.State.Map.Width || y < 0 || y >= _ctx.State.Map.Height) return false;
            var t = _ctx.State.Map.GetTileType(x, y);
            return t != TileType.Wall && t != TileType.ReinforcedWall && t != TileType.Door;
        }

        public void SafeCall(string system, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                _ctx.State.Errors.Report(system, ex);
                Logger.Error(system, $"Runtime error caught: {ex.Message}", ex);
            }
        }

        private void Notify(string text, Color? color = null) => _ctx.Hud.Notify(text, color);
    }
}