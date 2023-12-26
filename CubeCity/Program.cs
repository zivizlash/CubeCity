using Microsoft.Xna.Framework;
using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace CubeCity
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            var boundingBox = new BoundingBox();



            if (OperatingSystem.IsWindows())
                ConsoleTools.OpenConsole();

            using var game = new MainGame();
            game.Run();
        }
    }

    public static class ConsoleTools
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void OpenConsole()
        {
            var h = GetConsoleWindow();
            ShowWindow(h, 1);
        }
    }
}
