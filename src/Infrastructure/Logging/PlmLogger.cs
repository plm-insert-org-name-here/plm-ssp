using System.IO;
using Serilog;

namespace Infrastructure.Logging;

public static class PlmLogger
{
    private static string LogPath = "../../../plm-new/logs/PlmLogs.log";
    public static async void Log(string message)
    {
        using StreamWriter file = new(LogPath, append: true);
        await file.WriteLineAsync(message);
    }
}