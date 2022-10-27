using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.Database;

public static class DbInitializer
{

    public static void Initialize(IServiceScope scope)
    {
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (context.Database.CanConnect()) return;

        logger.Debug(
            "Database does not exist. Creating and initializing database from scratch...");
        context.Database.EnsureCreated();

        if (env.IsDevelopment())
        {
            logger.Debug("Loading seed data");
            new SeedLoader(context, env, logger).Load();
        }

        logger.Debug("Database initialization finished");
    }

    public static void SetInitialStates(IServiceScope scope)
    {
        var context = scope.ServiceProvider.GetRequiredService<Context>();

        // NOTE(rg): Raw SQL for efficient bulk update
        // https://docs.microsoft.com/en-us/ef/core/performance/efficient-updating
        // Proper support for bulk updates is coming in 7.0:
        // https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/plan#bulk-updates

        // Reset detectors to Off
        // context.Database.ExecuteSqlRaw("UPDATE Detectors SET State = 0");
        //
        // // Set Active tasks to Paused
        // context.Database.ExecuteSqlRaw("UPDATE Tasks SET Status = 1 WHERE Status = 0");
    }
}