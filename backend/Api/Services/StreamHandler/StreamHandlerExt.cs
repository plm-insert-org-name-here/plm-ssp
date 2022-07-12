using Microsoft.Extensions.DependencyInjection;

namespace Api.Services.StreamHandler
{
    public static class StreamHandlerExt
    {
        public static IServiceCollection AddStreamHandler(this IServiceCollection services)
        {
            services.AddHostedService<StreamHandlerService>();
            services.AddSingleton<StreamHandlerOpt>();
            services.AddSingleton<StreamViewerGroups>();

            return services;
        }
    }
}