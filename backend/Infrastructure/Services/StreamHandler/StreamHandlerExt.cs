using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services.StreamHandler
{
    public static class StreamHandlerExt
    {
        public static IServiceCollection AddStreamHandler(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StreamHandlerOpt>(
                configuration.GetSection(StreamHandlerOpt.SectionName));

            services.AddHostedService<StreamHandlerService>();
            services.AddSingleton<StreamHandlerOpt>();
            services.AddSingleton<StreamViewerGroups>();


            return services;
        }
    }
}