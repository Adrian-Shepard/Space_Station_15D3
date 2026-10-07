#nullable disable
using System;
using System.IO;

namespace SS15.Client.Core
{
    public enum LogLevel
    {
        Trace = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        None = 4
    }

    public static class Logger
    {
        public static LogLevel MinLevel = LogLevel.Trace;
        public static bool WriteToConsole = true;
        public static bool WriteToFile = true;

        private static readonly object _lock = new();
        private static string _logPath;

        public static void Init(string logFileName = "ss15_log.txt")
        {
            _logPath = Path.Combine(AppContext.BaseDirectory, logFileName);

            try
            {
                File.WriteAllText(_logPath,
                    $"=== SS15 Log started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
            }
            catch { }

            Info("Logger", $"Logging initialized, file: {_logPath}");
        }

        public static void Trace(string category, string message) => Write(LogLevel.Trace, category, message);
        public static void Info(string category, string message)  => Write(LogLevel.Info, category, message);
        public static void Warn(string category, string message)  => Write(LogLevel.Warn, category, message);
        public static void Error(string category, string message) => Write(LogLevel.Error, category, message);

        public static void Error(string category, string message, Exception ex)
        {
            Write(LogLevel.Error, category, $"{message} :: {ex.GetType().Name}: {ex.Message}");
            Write(LogLevel.Error, category, $"Stack: {ex.StackTrace}");
        }

        private static void Write(LogLevel level, string category, string message)
        {
            if (level < MinLevel) return;

            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] [{category}] {message}";

            lock (_lock)
            {
                if (WriteToConsole)
                {
                    var prev = Console.ForegroundColor;
                    Console.ForegroundColor = level switch
                    {
                        LogLevel.Error => ConsoleColor.Red,
                        LogLevel.Warn => ConsoleColor.Yellow,
                        LogLevel.Trace => ConsoleColor.DarkGray,
                        _ => ConsoleColor.Gray
                    };
                    Console.WriteLine(line);
                    Console.ForegroundColor = prev;
                }

                if (WriteToFile && _logPath != null)
                {
                    try { File.AppendAllText(_logPath, line + "\n"); }
                    catch { }
                }
            }
        }
    }
}