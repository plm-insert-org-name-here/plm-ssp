using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Api.Services.DetectorController
{
    public class DetectorController
    {
        private readonly ILogger _logger;
        private readonly DetectorControllerOpt _opt;
        private readonly StreamViewerGroups _groups;
        private readonly IDbContextFactory<Context> _contextFactory;
        private readonly DetectorCommandQueues _queues;
        private readonly SnapshotCache _snapshotCache;

        public DetectorController(
            IOptions<DetectorControllerOpt> opt,
            ILogger logger,
            DetectorCommandQueues queues,
            SnapshotCache snapshotCache,
            IDbContextFactory<Context> contextFactory, StreamViewerGroups groups)
        {
            _logger = logger;
            _queues = queues;
            _snapshotCache = snapshotCache;
            _contextFactory = contextFactory;
            _groups = groups;
            _opt = opt.Value;
        }

        // TODO(rg): cancellation tokens?
        public async Task HandleConnection(WebSocket webSocket)
        {
            var detectorAddress = await ReceiveMacAddress(webSocket);
            if (detectorAddress is null) return;

            var detectorId = await RegisterDetector(detectorAddress);

            await HandleCommandLoop(webSocket, detectorId);

            await DisconnectDetector(detectorId);
        }

        private async Task<PhysicalAddress?> ReceiveMacAddress(WebSocket webSocket)
        {
            var detectorAddressString = await ReceiveTextAsync(webSocket);
            try
            {
                return PhysicalAddress.Parse(detectorAddressString);
            }
            catch (FormatException)
            {
                _logger.Warning("Connection attempt by detector with invalid MAC address: '{Mac}'",
                    detectorAddressString);
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid MAC address",
                    CancellationToken.None);
                return null;
            }
        }

        private async Task DisconnectDetector(int detectorId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var detector = await context.Detectors.SingleOrDefaultAsync(d => d.Id == detectorId);

            _logger.Information("Detector (id: {Id}) disconnected", detectorId);
            _queues.RemoveQueue(detectorId);
            detector.State = DetectorState.Off;
            await context.SaveChangesAsync();
        }

        private async Task HandleCommandLoop(WebSocket webSocket, int detectorId)
        {
            var queue = _queues.AddQueue(detectorId);

            try
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.ASCII.GetBytes(detectorId.ToString())),
                    WebSocketMessageType.Text,
                    true,
                    new CancellationTokenSource(_opt.TimeoutMilliseconds).Token
                );

                while (true)
                {
                    while (queue.Count == 0)
                    {
                        // TODO(rg): use BlockingConnection<T> or smth
                        queue.EnqueueEvent.WaitOne(_opt.PingMilliseconds);
                        const DetectorCommandType ping = DetectorCommandType.Ping;
                        _logger.Debug("Sending '{Ping}' to detector (id: {Id})", ping, detectorId);
                        var pingResult = await SendCommandWithVerificationAsync(webSocket, ping);
                        if (!pingResult)
                        {
                            // NOTE(rg): Detector didn't respond to ping, which means it's disconnected
                            return;
                        }
                    }

                    var command = queue.Dequeue();
                    await using var context = _contextFactory.CreateDbContext();

                    var detector = await context.Detectors
                        .Where(d => d.Id == detectorId)
                        .Include(d => d.Location!)
                        .ThenInclude(l => l.Job)
                        .SingleOrDefaultAsync();

                    var result = CheckPreconditions(detector, command);
                    if (!result) continue;

                    _logger.Debug("Sending '{Cmd}' to detector (id: {Id})", command, detector.Id);
                    result = await SendCommandWithVerificationAsync(webSocket, command);
                    if (!result) continue;

                    await PerformStateChanges(webSocket, context, detector, command);
                }
            }
            catch (WebSocketException ex)
            {
                _logger.Information("Lost connection to detector (id: {Id}). WebSocket error code: {@WsErr}",
                    detectorId, ex.WebSocketErrorCode);
            }
        }

        private bool CheckPreconditions(Detector detector, DetectorCommand command)
        {
            if (command.Type is DetectorCommandType.TakeSnapshot)
            {
                return detector.Location is not null;
            }

            return true;
        }

        private async Task PerformStateChanges(
            WebSocket webSocket,
            Context context,
            Detector detector,
            DetectorCommand command)
        {
            switch (command.Type)
            {
                case DetectorCommandType.StartStreaming:
                {
                    if (detector.State is DetectorState.Standby)
                        detector.State = DetectorState.Streaming;
                    await context.SaveChangesAsync();
                    break;
                }
                case DetectorCommandType.StopStreaming:
                {
                    // If Monitoring is enabled, don't stop to stream
                    // (this would cripple Monitoring)
                    if (detector.State is DetectorState.Streaming)
                        detector.State = DetectorState.Standby;

                    await context.SaveChangesAsync();
                    break;
                }
                case DetectorCommandType.TakeSnapshot:
                {
                    var snapshot = await ReceiveSnapshotAsync(webSocket);
                    if (detector.Location is null)
                    {
                        // NOTE(rg): this is currently possible, e.g. if the Location was deleted
                        // after CheckPreconditions returned
                        break;
                    }

                    _snapshotCache.Set(detector.Location.Id, snapshot);
                    break;
                }
            }
        }

        private async Task<bool> SendCommandWithVerificationAsync(WebSocket webSocket, DetectorCommand command)
        {
            var expectedResponse = command.GetExpectedResponse();

            await SendCommandAsync(webSocket, command);
            var response = await ReceiveTextAsync(webSocket);

            var result = expectedResponse == response;

            if (result)
                _logger.Debug("Got expected response: '{Resp}'", expectedResponse);
            else
                _logger.Debug("Unexpected response: '{Unex}' (Expected '{Ex}')", response, expectedResponse);

            return result;
        }

        private async Task SendCommandAsync(WebSocket webSocket, DetectorCommand command)
        {
            try
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.ASCII.GetBytes(command.ToString())),
                    WebSocketMessageType.Text,
                    true,
                    new CancellationTokenSource(_opt.TimeoutMilliseconds).Token
                );
            }
            catch (OperationCanceledException ex)
            {
                _logger.Debug("DetectorController.Send timed out. Message: {Message}", ex.Message);
                throw new WebSocketException(WebSocketError.Faulted);
            }
        }

        private async Task<byte[]> ReceiveSnapshotAsync(WebSocket webSocket)
        {
            var image = new byte[_opt.SnapshotBufferSize];
            var tempBuffer = new byte[_opt.SnapshotBufferSize];
            var imageSize = 0;

            while (true)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(tempBuffer),
                        new CancellationTokenSource(_opt.TimeoutMilliseconds).Token);

                    if (webSocket.State == WebSocketState.CloseReceived)
                    {
                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null,
                            new CancellationTokenSource(_opt.TimeoutMilliseconds).Token);
                        throw new WebSocketException(WebSocketError.Success);
                    }

                    var size = result.Count;

                    // TODO: handle overflow?
                    Array.Copy(tempBuffer, 0, image, imageSize, size);
                    imageSize += size;

                    if (result.EndOfMessage)
                    {
                        return image.Take(imageSize).ToArray();
                    }
                }
                catch (OperationCanceledException ex)
                {
                    _logger.Warning("DetectorController.ReceiveSnapshot timed out. Message: {Message}", ex.Message);
                    throw new WebSocketException(WebSocketError.Faulted);
                }
            }
        }

        private async Task<string> ReceiveTextAsync(WebSocket webSocket)
        {
            var buffer = new byte[_opt.ResponseBufferSize];


            try
            {
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                    new CancellationTokenSource(_opt.TimeoutMilliseconds).Token);

                if (webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null,
                        new CancellationTokenSource(_opt.TimeoutMilliseconds).Token);
                    throw new WebSocketException(WebSocketError.Success);
                }

                var stringResponse = Encoding.Default.GetString(buffer).Replace("\0", "");
                return stringResponse;
            }
            catch (OperationCanceledException ex)
            {
                _logger.Debug("DetectorController.Receive timed out. Message: {Message}", ex.Message);
                throw new WebSocketException(WebSocketError.Faulted);
            }
        }


        private async Task<int> RegisterDetector(PhysicalAddress macAddress)
        {
            await using var context = _contextFactory.CreateDbContext();

            var existingDetector =
                await context.Detectors
                    .Where(d => d.MacAddress.Equals(macAddress))
                    .Include(d => d.Location)
                    .SingleOrDefaultAsync();
            if (existingDetector is not null)
            {
                _logger.Information("Detector (id: {Id}) connected", existingDetector.Id);
                existingDetector.State = DetectorState.Standby;
                await context.SaveChangesAsync();
                return existingDetector.Id;
            }

            var detector = new Detector
            {
                Name = macAddress.ToString(),
                MacAddress = macAddress,
                State = DetectorState.Standby,
            };

            await context.Detectors.AddAsync(detector);
            await context.SaveChangesAsync();
            _logger.Information("Registered new detector with MAC address '{Mac}' (id: {Id})", macAddress, detector.Id);

            return detector.Id;
        }
    }
}