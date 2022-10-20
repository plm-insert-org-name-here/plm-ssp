using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database;

public static class DbExt
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        Action<DbContextOptionsBuilder> dbOptions = options =>
        {
            options.EnableSensitiveDataLogging();

            var connString = configuration.GetConnectionString("Default");
            var serverVersion = ServerVersion.AutoDetect(connString);

            options.UseMySql(connString, serverVersion);
        };

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddDbContext<Context>(dbOptions);
        // services.AddDbContextFactory<Context>(dbOptions);

        return services;
    }

    public static void InitializeDatabase(this IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.CreateScope();

        DbInitializer.Initialize(scope);
        DbInitializer.SetInitialStates(scope);
    }


}