#nullable disable
using System;
using SS15.Client.Core;

namespace SS15.Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Logger.Init();
                Logger.Info("Program", "Application starting");

                using var game = new MainGame();
                game.Run();

                Logger.Info("Program", "Application exited normally");
            }
            catch (Exception ex)
            {
                Logger.Error("Program", "Unhandled exception", ex);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}