#nullable disable
using System;
using System.Numerics;
using Engine.Rendering;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Objects;
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
            if (_ctx.State.Map.GetTile(x, y) == TileType.Door)
                ToggleDoor(x, y);
            else
                PickupItem(x, y);
        }

        public void ToggleDoor(int x, int y)
        {
            int gid = _ctx.State.Map.GetDoorGroupId(x, y);
            if (gid <= 0) return;

            bool open = _ctx.State.Map.IsDoorOpen(x, y);
            bool newState = !open;

            if (!newState && AnyPlayerInDoorGroup(gid))
            {
                Notify("Дверь заблокирована — кто-то в проёме", new Color(1f, 0.5f, 0.5f, 1f));
                return;
            }

            _ctx.State.Map.SetDoorOpen(x, y, newState);
            var tiles = _ctx.State.Map.GetDoorTilesInGroup(gid);
            foreach (var (dx, dy) in tiles)
                _ctx.State.Atmosphere.SetDoorState(dx, dy, newState);

            Notify(newState ? "Дверь открыта" : "Дверь закрыта", new Color(1f, 0.85f, 0.4f, 1f));
        }

        public void PickupItem(int x, int y)
        {
            var items = _ctx.State.Map.GetItemsAtTile(x, y);
            if (items == null || items.Count == 0) return;

            var item = items[0];
            _ctx.State.Map.RemoveItemAtTile(x, y, item);
            if (!_ctx.State.Player.Inventory.AddToHand(item) &&
                !_ctx.State.Player.Inventory.AddToPocket(item) &&
                !_ctx.State.Player.Inventory.AddToBelt(item) &&
                !_ctx.State.Player.Inventory.AddToBack(item))
                _ctx.State.Map.AddItemToTile(x, y, item);
            else
                Notify($"Подобрано: {item.Name}", new Color(0.6f, 0.9f, 1f, 1f));
        }

        public void PlaceWall(int x, int y)
        {
            var tile = _ctx.State.Map.GetTile(x, y);
            if (tile == TileType.Wall || tile == TileType.ReinforcedWall || tile == TileType.Door) return;

            _ctx.State.Map.SetTile(x, y, TileType.Wall);
            _ctx.State.Atmosphere.SetWall(x, y, true);
            Notify($"Стена поставлена ({x},{y})");
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
                RegisterDoorTile(tx, ty, gid);
                _ctx.State.Atmosphere.SetDoorState(tx, ty, false);
            }

            Notify($"Дверь установлена ({x},{y})", new Color(0.6f, 1f, 0.6f, 1f));
        }

        public void PlaceWire(int x, int y) { SetTile(x, y, TileType.Wire); Notify($"Провод ({x},{y})"); }
        public void PlaceGenerator(int x, int y) { SetTile(x, y, TileType.Generator); Notify($"Генератор ({x},{y})", new Color(0.9f, 0.8f, 0.4f, 1f)); }
        public void PlaceLamp(int x, int y) { SetTile(x, y, TileType.Lamp); Notify($"Лампа ({x},{y})", new Color(0.9f, 0.9f, 0.5f, 1f)); }

        public void RemoveObject(int x, int y)
        {
            var tile = _ctx.State.Map.GetTile(x, y);
            if (tile == TileType.Floor)
            {
                _ctx.State.Map.SetTile(x, y, TileType.Space);
                _ctx.State.Atmosphere.SetTileToSpace(x, y);
                Notify($"Убрано в Space ({x},{y})", new Color(0.8f, 0.8f, 1f, 1f));
            }
            else if (tile == TileType.Space)
            {
                _ctx.State.Map.SetTile(x, y, TileType.Floor);
                _ctx.State.Atmosphere.SetTileToFloor(x, y);
                Notify($"Восстановлен пол ({x},{y})", new Color(0.7f, 1f, 0.7f, 1f));
            }
            else
            {
                _ctx.State.Map.SetTile(x, y, TileType.Floor);
                _ctx.State.Atmosphere.SetTileToFloor(x, y);
                Notify($"Удалено ({x},{y})", new Color(1f, 0.7f, 0.7f, 1f));
            }
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

        public void DropItem()
        {
            int h = _ctx.State.ActiveHandIndex;
            var item = _ctx.State.Player.Inventory.RemoveItem(h);
            if (item == null) return;

            bool ctrl = _ctx.Keyboard.IsKeyPressed(Key.ControlLeft) || _ctx.Keyboard.IsKeyPressed(Key.ControlRight);

            var p = _ctx.State.Player;

            if (ctrl)
            {
                float dirX = MathF.Cos(p.Direction);
                float dirY = MathF.Sin(p.Direction);
                var dir = new Vector2(dirX, dirY);

                var from = p.Position + dir * 10f;

                float sliding = SlidingFrictionFor(item.Type);

                _ctx.State.ItemFlight.Throw(item, from, dir, p.Pitch, 700f, sliding);
                Notify($"Брошено: {item.Name}", new Color(1f, 0.9f, 0.4f, 1f));
            }
            else
            {
                int tx = (int)(p.Position.X / _ctx.State.Map.TileSize);
                int ty = (int)(p.Position.Y / _ctx.State.Map.TileSize);
                _ctx.State.Map.AddItemToTile(tx, ty, item);
                Notify($"Выброшено: {item.Name}", new Color(1f, 0.85f, 0.6f, 1f));
            }
        }

        private static float SlidingFrictionFor(ItemType type)
        {
            return type switch
            {
                ItemType.Medkit => 4.0f,
                ItemType.Food   => 3.0f,
                ItemType.Tool   => 2.0f,
                _               => 3.5f,
            };
        }

        public void ModifyOxygen(int x, int y, float d) => _ctx.State.Atmosphere.ModifyOxygen(x, y, d);
        public void ModifyNitrogen(int x, int y, float d) => _ctx.State.Atmosphere.ModifyNitrogen(x, y, d);
        public void ModifyCarbonDioxide(int x, int y, float d) => _ctx.State.Atmosphere.ModifyCarbonDioxide(x, y, d);
        public void ModifyTemperature(int x, int y, float d) => _ctx.State.Atmosphere.ModifyTemperature(x, y, d);
        public void ModifyPlasma(int x, int y, float d) => _ctx.State.Atmosphere.ModifyPlasma(x, y, d);

        public bool AnyPlayerInDoorGroup(int gid)
        {
            var tiles = _ctx.State.Map.GetDoorTilesInGroup(gid);
            foreach (var (tx, ty) in tiles)
                if (IsPlayerInTile(tx, ty))
                    return true;
            return false;
        }

        public bool IsPlayerInTile(int tx, int ty)
        {
            var p = _ctx.State.Player;
            int ts = _ctx.State.Map.TileSize;
            float tileX0 = tx * ts;
            float tileY0 = ty * ts;
            float half = p.PlayerHalf;
            float px = p.Position.X;
            float py = p.Position.Y;

            return px + half > tileX0 && px - half < tileX0 + ts
                && py + half > tileY0 && py - half < tileY0 + ts;
        }

        private bool CanPlaceDoorTile(int x, int y)
        {
            if (x < 0 || x >= _ctx.State.Map.Width || y < 0 || y >= _ctx.State.Map.Height) return false;
            var t = _ctx.State.Map.GetTile(x, y);
            return t != TileType.Wall && t != TileType.ReinforcedWall && t != TileType.Door;
        }

        private void RegisterDoorTile(int x, int y, int groupId)
        {
            var obj = ObjectFactory.Create(TileType.Door);
            if (obj != null)
            {
                _ctx.State.Map.SetTile(x, y, obj.TileType);
                obj.OnPlaced(_ctx.State.Map, x, y);
            }
            else
            {
                _ctx.State.Map.SetTile(x, y, TileType.Door);
            }
            _ctx.State.Map.RegisterDoorTile(x, y, groupId);
        }

        private void SetTile(int x, int y, TileType type)
        {
            if (_ctx.State.Map.GetTile(x, y) != TileType.Wall &&
                _ctx.State.Map.GetTile(x, y) != TileType.ReinforcedWall &&
                _ctx.State.Map.GetTile(x, y) != TileType.Door)
            {
                var obj = ObjectFactory.Create(type);
                if (obj != null)
                {
                    _ctx.State.Map.SetTile(x, y, obj.TileType);
                    obj.OnPlaced(_ctx.State.Map, x, y);
                }
            }
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