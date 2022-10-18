using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services.ProcessorHandler
{
    public static class ProcessorHandlerExt
    {
        public static IServiceCollection AddProcessorHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ProcessorHandlerOpt>(
                configuration.GetSection(ProcessorHandlerOpt.SectionName));

            services.AddHostedService<PacketReceiverService>();
            services.AddSingleton<PacketSender>();

            return services;
        }
    }
}