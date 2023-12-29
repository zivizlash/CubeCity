using Microsoft.Extensions.Logging;
using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace CubeCity;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        if (OperatingSystem.IsWindows())
            ConsoleTools.OpenConsole();

        using var game = new MainGame(CreateLoggerFactory());
        game.Run();
    }

    private static ILoggerFactory CreateLoggerFactory() => 
        LoggerFactory.Create(b => b.AddConsole());
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
