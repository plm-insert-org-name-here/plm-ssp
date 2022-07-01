using Microsoft.Extensions.DependencyInjection;

namespace Api.Services.DetectorStreamProcessor
{
    public static class DetectorStreamProcessorExtensions
    {
        public static IServiceCollection AddDetectorStreamProcessor(this IServiceCollection services)
        {
            services.AddHostedService<DetectorStreamProcessor>();
            services.AddSingleton<DetectorStreamProcessorOptions>();

            return services;
        }
    }
}