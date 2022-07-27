using System.Text.Json.Serialization;
using Api.Infrastructure.Database;
using Api.Services;
using Api.Services.DetectorController;
using Api.Services.MonitoringHandler;
using Api.Services.ProcessorHandler;
using Api.Services.StreamHandler;
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

            services.AddSingleton<SnapshotCache>();
            services.AddSingleton<MonitoringHandler>();
            services.AddDetectorControllerWebSocket(Configuration);
            services.AddStreamHandler(Configuration);
            services.AddProcessorHandler(Configuration);

            services.AddSignalR();
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

            app.UseCors(options =>
            {
                options.AllowAnyMethod();
                options.AllowAnyHeader();
                // NOTE(rg): workaround; the JS SignalR requires credentials to be allowed,
                // but AllowAnyOrigin and AllowCredentials can't be used together
                options.SetIsOriginAllowed(_ => true);
                // options.AllowAnyOrigin();
                options.AllowCredentials();
            });


            mapperConfiguration.AssertConfigurationIsValid();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseWebSockets();
            app.UseDetectorControllerWebSocket();

            app.UseEndpoints(builder =>
                {
                    builder.MapControllers();

                    builder.MapHub<StreamHub>(Routes.Detectors.DetectorHub);
                } );
        }
    }
}