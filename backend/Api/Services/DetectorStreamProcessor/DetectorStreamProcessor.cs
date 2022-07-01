using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Api.Services.DetectorController;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api.Services.DetectorStreamProcessor
{
    public class DetectorStreamProcessor : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly DetectorStreamProcessorOptions _options;

        public DetectorStreamProcessor(ILogger logger, IConfiguration configuration,
            DetectorStreamProcessorOptions options)
        {
            _logger = logger;
            _options = options;

            configuration.GetSection(DetectorStreamProcessorOptions.SectionName).Bind(_options);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunServer(stoppingToken);
        }

        private async Task RunServer(CancellationToken stoppingToken)
        {
            try
            {
                var client = new UdpClient(9697);

                var buffer = new byte[_options.FrameBufferSize];
                var bufferIndex = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = await client.ReceiveAsync();
                    var id = BitConverter.ToInt32(result.Buffer.Take(4).ToArray());
                    _logger.Debug("id: {Id}", id);
                    if (result.Buffer.Length != 4)
                    {
                        Array.Copy(result.Buffer, 4, buffer, bufferIndex, result.Buffer.Length - 4);
                        bufferIndex += result.Buffer.Length - 4;
                    }
                    else
                    {
                        var image = buffer.Take(bufferIndex).ToArray();
                        // TODO: do stuff with image
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Ex: {@Ex}", ex);
            }
        }
    }
}