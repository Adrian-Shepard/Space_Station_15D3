using System;
using System.Collections.Generic;
using SS15.Client.Core;

namespace SS15.Client.Objects
{
    public static class ObjectFactory
    {
        private static readonly Dictionary<TileType, Func<TileObjectBase>> _creators = new()
        {
            { TileType.Floor, () => new Floors.FloorStandard() },
            { TileType.MetalFloor, () => new Floors.FloorMetal() },
            { TileType.Wall, () => new Walls.WallStandard() },
            { TileType.ReinforcedWall, () => new Walls.WallReinforced() },
            { TileType.Wire, () => new Machinery.Wire() },
            { TileType.Generator, () => new Machinery.Generator() },
            { TileType.Lamp, () => new Machinery.Lamp() },
            { TileType.Door, () => new Machinery.Door() } // добавлено
        };

        public static TileObjectBase Create(TileType type)
        {
            return _creators.TryGetValue(type, out var creator) ? creator() : null;
        }
    }
}