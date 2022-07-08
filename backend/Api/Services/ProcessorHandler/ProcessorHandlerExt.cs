using System;
using Api.Services.DetectorController;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Services.ProcessorHandler
{
    public static class ProcessorHandlerExt
    {
        public static IServiceCollection AddProcessorHandler(this IServiceCollection services)
        {
            services.AddSingleton<ProcessorHandler>();
            services.AddSingleton<ProcessorHandlerOpt>();

            return services;
        }
    }
}