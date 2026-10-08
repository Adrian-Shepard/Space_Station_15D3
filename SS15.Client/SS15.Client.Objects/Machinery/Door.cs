using SS15.Client.Core;
using SS15.Client.Objects;

namespace SS15.Client.Objects.Machinery
{
    public class Door : TileObjectBase
    {
        public override TileType TileType => TileType.Door;
        public override string Name => "Door";
        public override bool IsSolid => false; // будет блокировать, когда закрыта

        public bool IsOpen { get; set; } = false; // по умолчанию закрыта

        public override void OnPlaced(GameMap map, int x, int y)
        {
            // При установке дверь закрыта
            IsOpen = false;
        }

        public void Toggle(GameMap map, int x, int y)
        {
            IsOpen = !IsOpen;
            // Обновляем проходимость
            map.SetTile(x, y, TileType.Door); // просто обновляем, но проходимость зависит от состояния
        }
    }
}