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
            services.AddDbContext<Context>(options =>
            {
                options.EnableSensitiveDataLogging();

                var connString = configuration.GetConnectionString("Default");
                var serverVersion = ServerVersion.AutoDetect(connString);

                options.UseMySql(connString, serverVersion);
            });
        }

        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.CreateScope();
            DbInitializer.Initialize(scope);
        }
    }
}