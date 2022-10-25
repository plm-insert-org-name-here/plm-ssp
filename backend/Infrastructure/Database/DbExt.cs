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
        var dbConfigSection = configuration.GetSection(DbOpt.SectionName);
        services.Configure<DbOpt>(dbConfigSection);

        Action<DbContextOptionsBuilder> dbOptions = options =>
        {
            options.EnableSensitiveDataLogging();

            var config = dbConfigSection.Get<DbOpt>();

            var serverVersion = ServerVersion.AutoDetect(config.ConnectionString);

            options.UseMySql(config.ConnectionString, serverVersion);
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