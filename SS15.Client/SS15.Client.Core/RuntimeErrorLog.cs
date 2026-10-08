using System;
using System.Collections.Generic;

namespace SS15.Client.Core
{
    public enum RuntimeErrorSeverity
    {
        Warning = 0,
        Error = 1,
        Critical = 2,
    }

    public class RuntimeErrorEntry
    {
        public DateTime WallClock;
        public double GameSeconds;   // сколько игрового времени прошло к моменту ошибки
        public string System;
        public string Message;
        public string ExceptionType;
        public string StackTrace;
        public RuntimeErrorSeverity Severity;

        public string ShortWallTime => WallClock.ToString("HH:mm:ss");
        public string ShortGameTime
        {
            get
            {
                int total = (int)GameSeconds;
                int h = total / 3600;
                int m = (total % 3600) / 60;
                int s = total % 60;
                return $"{h:00}:{m:00}:{s:00}";
            }
        }
    }

    public class RuntimeErrorLog
    {
        public const int MaxEntries = 64;

        private readonly List<RuntimeErrorEntry> _entries = new(MaxEntries);
        private readonly Dictionary<string, int> _countsBySystem = new();

        public int TotalCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ErrorCount { get; private set; }
        public int CriticalCount { get; private set; }

        /// Установить GameRuntime, чтобы записи знали текущее игровое время.
        public GameRuntime Runtime { get; set; }

        public event Action<RuntimeErrorEntry> OnNewError;

        public IReadOnlyList<RuntimeErrorEntry> Entries => _entries;
        public IReadOnlyDictionary<string, int> CountsBySystem => _countsBySystem;

        public bool HasErrors => TotalCount > 0;

        public void Report(string system, Exception ex, RuntimeErrorSeverity severity = RuntimeErrorSeverity.Error)
        {
            var entry = new RuntimeErrorEntry
            {
                WallClock = DateTime.Now,
                GameSeconds = Runtime?.GameSeconds ?? 0,
                System = system,
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = TrimStack(ex.StackTrace),
                Severity = severity,
            };
            Push(entry);
        }

        public void Report(string system, string message, RuntimeErrorSeverity severity = RuntimeErrorSeverity.Warning)
        {
            var entry = new RuntimeErrorEntry
            {
                WallClock = DateTime.Now,
                GameSeconds = Runtime?.GameSeconds ?? 0,
                System = system,
                Message = message,
                ExceptionType = "",
                StackTrace = "",
                Severity = severity,
            };
            Push(entry);
        }

        private void Push(RuntimeErrorEntry entry)
        {
            _entries.Add(entry);
            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0);

            TotalCount++;
            switch (entry.Severity)
            {
                case RuntimeErrorSeverity.Warning:  WarningCount++;  break;
                case RuntimeErrorSeverity.Error:    ErrorCount++;    break;
                case RuntimeErrorSeverity.Critical: CriticalCount++; break;
            }

            _countsBySystem.TryGetValue(entry.System, out int c);
            _countsBySystem[entry.System] = c + 1;

            try { OnNewError?.Invoke(entry); } catch { }
        }

        public void Clear()
        {
            _entries.Clear();
            _countsBySystem.Clear();
            TotalCount = WarningCount = ErrorCount = CriticalCount = 0;
        }

        private static string TrimStack(string stack)
        {
            if (string.IsNullOrEmpty(stack)) return "";
            int lineCount = 0;
            int idx = 0;
            while (idx < stack.Length && lineCount < 3)
            {
                if (stack[idx] == '\n') lineCount++;
                idx++;
            }
            return idx < stack.Length ? stack.Substring(0, idx) + "..." : stack;
        }
    }
}