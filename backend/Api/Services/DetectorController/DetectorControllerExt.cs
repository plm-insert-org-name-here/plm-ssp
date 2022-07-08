using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Services.DetectorController
{
    public static class DetectorControllerExt
    {
        public static IApplicationBuilder UseDetectorControllerWebSocket(this IApplicationBuilder app)
        {
            return app != null
                ? app.UseMiddleware<DetectorControllerMw>()
                : throw new ArgumentException(null, nameof(app));
        }

        public static IServiceCollection AddDetectorControllerWebSocket(this IServiceCollection services)
        {
            services.AddSingleton<DetectorCommandQueues>();
            services.AddScoped<DetectorController>();
            services.AddSingleton<DetectorControllerOpt>();

            return services;
        }
    }
}