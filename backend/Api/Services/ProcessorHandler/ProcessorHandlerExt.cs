using System;
using Api.Services.DetectorController;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Services.ProcessorHandler
{
    public static class ProcessorHandlerExt
    {
        public static IServiceCollection AddProcessorHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ProcessorHandlerOpt>(
                configuration.GetSection(ProcessorHandlerOpt.SectionName));

            services.AddHostedService<PacketReceiverService>();
            services.AddSingleton<PacketSender>();
            services.AddSingleton<ProcessorSocket>();

            return services;
        }
    }
}