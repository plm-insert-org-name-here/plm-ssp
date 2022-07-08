using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api.Infrastructure.Logging
{
    public static class LoggingExt
    {
        public static void UseLogging(this IHostBuilder builder)
        {
            // TODO: write to log file
            // WriteTo.File(logPath, RollingInterval.Hour)
            builder.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
            );
        }

    }
}