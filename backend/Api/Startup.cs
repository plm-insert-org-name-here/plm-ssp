using System.Text.Json.Serialization;
using Api.Infrastructure.Database;
using Api.Services;
using Api.Services.DetectorController;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddHostedService<DetectorImageProcessorService>();

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddValidatorsFromAssemblyContaining<Startup>();
            services.AddDatabase(Configuration);
            services.AddAutoMapper(typeof(Startup));
            services.AddAuthorization();
            services.AddSwaggerGen(configuration =>
            {
                configuration.EnableAnnotations();
                configuration.CustomSchemaIds(x => x.FullName);
            });

            services.AddSingleton<LocationSnapshotCache>();
            services.AddDetectorControllerWebSocket();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IConfigurationProvider mapperConfiguration)
        {
            if (env.IsDevelopment())
            {
                app.InitializeDatabase();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PLM API V1");
                });
            }

            mapperConfiguration.AssertConfigurationIsValid();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseWebSockets();
            app.UseDetectorControllerWebSocket();

            app.UseEndpoints(builder => builder.MapControllers());
        }
    }
}