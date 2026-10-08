using System;

namespace SS15.Client.Core
{

    public class GameRuntime
    {
        public double RealSeconds { get; private set; }
        public double GameSeconds { get; private set; }
        public long FrameCount { get; private set; }
        public float CurrentFps { get; private set; }

        public DateTime StartTimeLocal { get; }

        public GameRuntime()
        {
            StartTimeLocal = DateTime.Now;
        }

        public void TickReal(double deltaTime) => RealSeconds += deltaTime;
        public void TickGame(double step) => GameSeconds += step;
        public void NextFrame() => FrameCount++;
        public void SetFps(float fps) => CurrentFps = fps;

        public string FormatRealTime() => Format(RealSeconds);
        public string FormatGameTime() => Format(GameSeconds);

        private static string Format(double seconds)
        {
            if (seconds < 0) seconds = 0;
            int total = (int)seconds;
            int h = total / 3600;
            int m = (total % 3600) / 60;
            int s = total % 60;
            return $"{h:00}:{m:00}:{s:00}";
        }
    }
}