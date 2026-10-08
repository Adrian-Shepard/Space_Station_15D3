#nullable disable
using System.Collections.Generic;

namespace SS15.Client.Player
{

    public class PlayerEntityList
    {
        private readonly Dictionary<int, PlayerEntity> _byId = new();

        public IReadOnlyDictionary<int, PlayerEntity> All => _byId;

        public PlayerEntity Get(int id) => _byId.TryGetValue(id, out var e) ? e : null;

        public void Add(PlayerEntity e) => _byId[e.Id] = e;

        public void Remove(int id) => _byId.Remove(id);

        public void Clear() => _byId.Clear();
    }
}