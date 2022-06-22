using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api.Services.DetectorImageProcessor
{
    public class DetectorImageProcessorService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public DetectorImageProcessorService(ILogger logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunServer(stoppingToken);
        }

        private async Task RunServer(CancellationToken stoppingToken)
        {
            var client = new UdpClient(9500);

            while (!stoppingToken.IsCancellationRequested)
            {
                // TODO: each datagram is an image?
                // buffer size?

            }
        }
    }
}