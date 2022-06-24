using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.Database
{
    public static class DbServiceExtensions
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            Action<DbContextOptionsBuilder> dbOptions = options =>
            {
                options.EnableSensitiveDataLogging();

                var connString = configuration.GetConnectionString("Default");
                var serverVersion = ServerVersion.AutoDetect(connString);

                options.UseMySql(connString, serverVersion);

            };

            services.AddDbContext<Context>(dbOptions, optionsLifetime: ServiceLifetime.Singleton);
            services.AddDbContextFactory<Context>(dbOptions);
        }

        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.CreateScope();

            DbInitializer.Initialize(scope);
            DbInitializer.ResetDetectorStates(scope);
        }


    }
}