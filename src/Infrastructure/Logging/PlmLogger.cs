using System.IO;
using Serilog;

namespace Infrastructure.Logging;

public static class PlmLogger
{
    private static string LogPath = "../../../plm-ssp/logs/PlmLogs.log";
    public static async void Log(string? message)
    {
        if (message is not null)
        {
            using(StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(message);
            }
        }
        
    }
}
