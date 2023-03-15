using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.Logging;

public static class LoggingExt
{
    private static string LogPath = "../../../plm-new/logs/SeriLogs.log";
    public static void UseLogging(this IHostBuilder builder)
    {
        // TODO: write to log file
        // WriteTo.File(logPath, RollingInterval.Hour)
        builder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.File(LogPath)
        );
    }

}