using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database;

public static class DbExt
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var dbConfigSection = config.GetSection(DbOpt.SectionName);
        services.Configure<DbOpt>(dbConfigSection);

        var dbOpt = dbConfigSection.Get<DbOpt>();
        Console.WriteLine("Connection string: " + dbOpt.ConnectionString);

        Action<DbContextOptionsBuilder> dbOptions = options =>
        {
            options.EnableSensitiveDataLogging();


            var serverVersion = ServerVersion.AutoDetect(dbOpt.ConnectionString);
            options.UseMySql(dbOpt.ConnectionString, serverVersion, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        };

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddDbContext<Context>(dbOptions);

        return services;
    }

    public static void InitializeDatabase(this IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.CreateScope();

        DbInitializer.Initialize(scope);
        DbInitializer.SetInitialStates(scope);
    }


}
