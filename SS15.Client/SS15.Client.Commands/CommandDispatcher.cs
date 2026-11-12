using System;
using System.Collections.Generic;

namespace SS15.Client.Commands
{
    public class CommandDispatcher
    {
        private readonly GameContext _ctx;
        private readonly List<GameCommand> _history = new();

        public int MaxHistory { get; set; } = 2000;
        public long TotalDispatched { get; private set; }
        public long TotalRejected { get; private set; }

        public IReadOnlyList<GameCommand> History => _history;

        public CommandDispatcher(GameContext ctx)
        {
            _ctx = ctx;
        }

        public GameCommandResult Dispatch(GameCommand cmd)
        {
            var result = cmd.Apply(_ctx);

            if (result.Success)
            {
                _history.Add(cmd);
                if (_history.Count > MaxHistory)
                    _history.RemoveAt(0);
                TotalDispatched++;
            }
            else
            {
                TotalRejected++;
            }

            return result;
        }

        public void ClearHistory() => _history.Clear();
    }
}