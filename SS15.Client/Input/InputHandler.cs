#nullable disable
using System;
using System.Numerics;
using Silk.NET.Input;
using SS15.Client.Core;
using SS15.Client.Player;

namespace SS15.Client.Input
{
    public class InputHandler
    {
        private IKeyboard _keyboard;
        private IMouse _mouse;
        private GameMap _map;
        private PlayerController _player;

        public event Action<int, int> LeftClick;
        public event Action<int, int> PlaceWall;
        public event Action<int, int> PlaceDoor;
        public event Action<int, int> PlaceWire;
        public event Action<int, int> PlaceGenerator;
        public event Action<int, int> PlaceLamp;
        public event Action<int, int> RemoveObject;
        public event Action SwitchHand;
        public event Action PlaceInPocket;
        public event Action UseItem;
        public event Action DropItem;
        public event Action<int> SelectSlot;
        public event Action<int, int, float> ModifyOxygen;
        public event Action<int, int, float> ModifyNitrogen;
        public event Action<int, int, float> ModifyCarbonDioxide;
        public event Action<int, int, float> ModifyTemperature;

        private bool _leftPrev;
        private bool _qPrev, _ePrev, _rPrev;
        private bool _tPrev, _bPrev, _xPrev;
        private bool _zPrev, _vPrev, _cPrev, _gPrev;
        private bool[] _slotPrev = new bool[10];

        private Vector2 _lastMousePos;
        private bool _mouseInitialized;
        private const float MouseSens = 0.003f;
        private const float PitchLimit = 1.4f;

        public InputHandler(IKeyboard keyboard, IMouse mouse, GameMap map, PlayerController player)
        {
            _keyboard = keyboard;
            _mouse = mouse;
            _map = map;
            _player = player;

            try { _mouse.Cursor.CursorMode = CursorMode.Raw; } catch { }
        }

        public void Update(float deltaTime)
        {
<<<<<<< Updated upstream
            Vector2 mouseScreen = new Vector2(_mouse.Position.X, _mouse.Position.Y);
            Vector2 cameraPos = _getCameraPosition();
            float zoom = _getZoom();
            Vector2 scale = _getScale();

            // Экранные координаты -> виртуальные -> мировые
            Vector2 virtualScreen = mouseScreen / scale; // делим на масштаб, получаем виртуальные координаты
            Vector2 mouseWorld = cameraPos + virtualScreen / zoom;
            int mouseTileX = (int)(mouseWorld.X / _map.TileSize);
            int mouseTileY = (int)(mouseWorld.Y / _map.TileSize);

            // Левая кнопка мыши
            if (_mouse.IsButtonPressed(MouseButton.Left) && !_leftPrev)
=======
            // Мышь
            Vector2 curMouse = new Vector2(_mouse.Position.X, _mouse.Position.Y);
            if (!_mouseInitialized)
>>>>>>> Stashed changes
            {
                _lastMousePos = curMouse;
                _mouseInitialized = true;
            }

            float dx = curMouse.X - _lastMousePos.X;
            float dy = curMouse.Y - _lastMousePos.Y;
            _lastMousePos = curMouse;

            if (MathF.Abs(dx) > 0.01f) _player.Direction += dx * MouseSens;
            if (MathF.Abs(dy) > 0.01f)
                _player.Pitch = Math.Clamp(_player.Pitch - dy * MouseSens, -PitchLimit, PitchLimit);

            // Стрелки
            if (_keyboard.IsKeyPressed(Key.Left)) _player.Direction -= 2f * deltaTime;
            if (_keyboard.IsKeyPressed(Key.Right)) _player.Direction += 2f * deltaTime;
            if (_keyboard.IsKeyPressed(Key.Up))
                _player.Pitch = Math.Clamp(_player.Pitch + 2f * deltaTime, -PitchLimit, PitchLimit);
            if (_keyboard.IsKeyPressed(Key.Down))
                _player.Pitch = Math.Clamp(_player.Pitch - 2f * deltaTime, -PitchLimit, PitchLimit);

            // Тайл перед игроком
            float lookDist = 32f * 0.6f;
            int tx = (int)((_player.Position.X + MathF.Cos(_player.Direction) * lookDist) / _map.TileSize);
            int ty = (int)((_player.Position.Y + MathF.Sin(_player.Direction) * lookDist) / _map.TileSize);

            if (_mouse.IsButtonPressed(MouseButton.Left) && !_leftPrev)
                LeftClick?.Invoke(tx, ty);
            _leftPrev = _mouse.IsButtonPressed(MouseButton.Left);

            if (IsPressed(Key.Q, ref _qPrev)) DropItem?.Invoke();
            if (IsPressed(Key.E, ref _ePrev)) PlaceInPocket?.Invoke();
            if (IsPressed(Key.R, ref _rPrev)) UseItem?.Invoke();
            if (IsPressed(Key.T, ref _tPrev)) PlaceWall?.Invoke(tx, ty);
            if (IsPressed(Key.B, ref _bPrev)) PlaceDoor?.Invoke(tx, ty);
            if (IsPressed(Key.X, ref _xPrev)) SwitchHand?.Invoke();
            if (IsPressed(Key.Z, ref _zPrev)) PlaceWire?.Invoke(tx, ty);
            if (IsPressed(Key.V, ref _vPrev)) PlaceGenerator?.Invoke(tx, ty);
            if (IsPressed(Key.C, ref _cPrev)) PlaceLamp?.Invoke(tx, ty);
            if (IsPressed(Key.G, ref _gPrev)) RemoveObject?.Invoke(tx, ty);

<<<<<<< Updated upstream
            // Выбор активного слота
=======
>>>>>>> Stashed changes
            for (int i = 0; i <= 9; i++)
                if (IsPressed(Key.Number0 + i, ref _slotPrev[i]))
                    SelectSlot?.Invoke(i);

<<<<<<< Updated upstream
            // Модификаторы атмосферы (зажатие)
            if (_keyboard.IsKeyPressed(Key.U))
                ModifyOxygen?.Invoke(mouseTileX, mouseTileY, 1f);
            if (_keyboard.IsKeyPressed(Key.J))
                ModifyOxygen?.Invoke(mouseTileX, mouseTileY, -1f);
            if (_keyboard.IsKeyPressed(Key.O))
                ModifyNitrogen?.Invoke(mouseTileX, mouseTileY, 1f);
            if (_keyboard.IsKeyPressed(Key.L))
                ModifyNitrogen?.Invoke(mouseTileX, mouseTileY, -1f);
            if (_keyboard.IsKeyPressed(Key.N))
                ModifyCarbonDioxide?.Invoke(mouseTileX, mouseTileY, 0.1f);
            if (_keyboard.IsKeyPressed(Key.M))
                ModifyCarbonDioxide?.Invoke(mouseTileX, mouseTileY, -0.1f);
            if (_keyboard.IsKeyPressed(Key.I))
                ModifyTemperature?.Invoke(mouseTileX, mouseTileY, 10f);
            if (_keyboard.IsKeyPressed(Key.K))
                ModifyTemperature?.Invoke(mouseTileX, mouseTileY, -10f);
=======
            if (_keyboard.IsKeyPressed(Key.U)) ModifyOxygen?.Invoke(tx, ty, 1f);
            if (_keyboard.IsKeyPressed(Key.J)) ModifyOxygen?.Invoke(tx, ty, -1f);
            if (_keyboard.IsKeyPressed(Key.O)) ModifyNitrogen?.Invoke(tx, ty, 1f);
            if (_keyboard.IsKeyPressed(Key.L)) ModifyNitrogen?.Invoke(tx, ty, -1f);
            if (_keyboard.IsKeyPressed(Key.N)) ModifyCarbonDioxide?.Invoke(tx, ty, 0.1f);
            if (_keyboard.IsKeyPressed(Key.M)) ModifyCarbonDioxide?.Invoke(tx, ty, -0.1f);
            if (_keyboard.IsKeyPressed(Key.I)) ModifyTemperature?.Invoke(tx, ty, 10f);
            if (_keyboard.IsKeyPressed(Key.K)) ModifyTemperature?.Invoke(tx, ty, -10f);
            if (_keyboard.IsKeyPressed(Key.Y)) ModifyPlasma?.Invoke(tx, ty, 1f);
            if (_keyboard.IsKeyPressed(Key.H)) ModifyPlasma?.Invoke(tx, ty, -1f);
>>>>>>> Stashed changes
        }

        private bool IsPressed(Key key, ref bool prev)
        {
            bool cur = _keyboard.IsKeyPressed(key);
            bool res = cur && !prev;
            prev = cur;
            return res;
        }
    }
}