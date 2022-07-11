using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Services.ProcessorHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Api.Services.StreamHandler
{
    public class StreamHandler : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly StreamHandlerOpt _opt;
        private readonly StreamViewerGroups _groups;
        private readonly ProcessorHandler.ProcessorHandler _processor;
        private readonly IDbContextFactory<Context> _contextFactory;

        public StreamHandler(
            ILogger logger,
            IConfiguration configuration,
            StreamHandlerOpt opt,
            StreamViewerGroups groups,
            IDbContextFactory<Context> contextFactory,
            ProcessorHandler.ProcessorHandler processor)
        {
            _logger = logger;
            _opt = opt;
            _groups = groups;
            _contextFactory = contextFactory;
            _processor = processor;

            configuration.GetSection(StreamHandlerOpt.SectionName).Bind(_opt);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // We need a separate method for this
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0#backgroundservice-base-class
            await RunServer(stoppingToken);
        }

        private class DetectorFrameBuffer
        {
            public byte[] Buffer { get; set; } = default!;
            public int Index { get; set; }
        }

        private async Task RunServer(CancellationToken stoppingToken)
        {
            try
            {
                var client = new UdpClient(_opt.UdpPort);
                // Because frames from detectors come in tiny datagrams, and all communication is multiplexed into this
                // single connection, we need buffers for each detector for storing the incomplete frames
                var frameBuffers = new Dictionary<int, DetectorFrameBuffer>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = await client.ReceiveAsync();
                    var detectorId = BitConverter.ToInt32(result.Buffer.Take(4).ToArray());
                    DetectorFrameBuffer frameBuffer;

                    if (!frameBuffers.ContainsKey(detectorId))
                    {
                        frameBuffer = new DetectorFrameBuffer
                        {
                            Buffer = new byte[_opt.FrameBufferSize],
                            Index = 0
                        };
                        frameBuffers.Add(detectorId, frameBuffer);
                    }
                    else
                    {
                        frameBuffer = frameBuffers[detectorId];
                    }

                    // Received an "empty" datagram, which only contains the detector id -> frame is complete
                    if (result.Buffer.Length == 4)
                    {
                        var frame = frameBuffer.Buffer.Take(frameBuffer.Index).ToArray();
                        frameBuffer.Index = 0;

                        // TODO(rg): cache detector states to eliminate roundtrip to DB on every frame
                        await using var context = _contextFactory.CreateDbContext();
                        var detector = await context.Detectors
                            .Where(d => d.Id == detectorId)
                            .SingleOrDefaultAsync(stoppingToken);

                        if (detector.State is DetectorState.Monitoring) {
                            var framePacket = new FramePacket(detectorId, frame.Length, frame);
                            await _processor.SendFrame(framePacket);
                        }

                        // We don't need to check for subs here as the method already does
                        await _groups.SendStreamFrameToGroup(detectorId, frame);
                    }
                    else
                    {
                        Array.Copy(result.Buffer, 4, frameBuffer.Buffer, frameBuffer.Index, result.Buffer.Length - 4);
                        frameBuffer.Index += result.Buffer.Length - 4;
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