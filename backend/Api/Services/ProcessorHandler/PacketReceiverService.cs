using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api.Services.ProcessorHandler
{
    public class PacketReceiverService : BackgroundService
    {
        private readonly ProcessorSocket _processorSocket;
        private readonly ProcessorHandlerOpt _opt;
        private readonly ILogger _logger;

        public PacketReceiverService(ProcessorSocket processorSocket, ProcessorHandlerOpt opt, ILogger logger)
        {
            _processorSocket = processorSocket;
            _opt = opt;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunReceiver(stoppingToken);
        }

        private async Task RunReceiver(CancellationToken stoppingToken)
        {
            // TODO(rg): run continuously, read ResultPackets from processor socket,
            // and perform business logic
            while (true)
            {
            }
        }
    }
}