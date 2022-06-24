using System.Net.NetworkInformation;
using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Api.Infrastructure.Database
{
    public static class DbInitializer
    {
        private static Location[] InitialLocations =
        {
            new()
            {
                Name = "New Location 1"
            },
            new()
            {
                Name = "New Location 2"
            },
            new()
            {
                Name = "New Location 3"
            },
            new()
            {
                Name = "New Location 4"
            },
            new()
            {
                Name = "New Location 5"
            },
            new()
            {
                Name = "New Location 6"
            }
        };

        private static Detector[] InitialDetectors =
        {
            new()
            {
                Name = "New Detector 1",
                MacAddress = PhysicalAddress.Parse("1234567890AB")
            },
            new()
            {
                Name = "New Detector 2",
                MacAddress = PhysicalAddress.Parse("12AB34CD56EF")
            },
            new()
            {
                Name = "New Detector 3",
                MacAddress = PhysicalAddress.Parse("123123123123")
            }
        };

        public static void Initialize(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

            if (context.Database.CanConnect()) return;

            logger.Debug("Database does not exist. Creating and initializing database from scratch...");

            context.Database.EnsureCreated();

            context.Locations.AddRange(InitialLocations);
            context.Detectors.AddRange(InitialDetectors);

            context.SaveChanges();
            logger.Debug("Database initialization finished");
        }

        public static void ResetDetectorStates(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<Context>();

            // NOTE(rg): Raw SQL for efficient bulk update
            // https://docs.microsoft.com/en-us/ef/core/performance/efficient-updating
            // Proper support for bulk updates is coming in 7.0:
            // https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/plan#bulk-updates
            context.Database.ExecuteSqlRaw("UPDATE Detectors SET State = 0");
        }
    }
}