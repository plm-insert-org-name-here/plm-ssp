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
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Api.Services.DetectorController
{
    public class DetectorController
    {
        private readonly ILogger _logger;
        private readonly DetectorControllerOptions _options;
        private readonly Context _context;
        private readonly DetectorCommandQueues _queues;
        private readonly DetectorSnapshotCache _snapshotCache;

        public DetectorController(
            IConfiguration configuration,
            DetectorControllerOptions options,
            ILogger logger,
            Context context,
            DetectorCommandQueues queues,
            DetectorSnapshotCache snapshotCache)
        {
            _logger = logger;
            _context = context;
            _queues = queues;
            _snapshotCache = snapshotCache;

            // not sure if this works in the ctor
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#bind-hierarchical-configuration-data-using-the-options-pattern
            _options = options;
            configuration.GetSection(DetectorControllerOptions.SectionName).Bind(_options);
        }

        // TODO(rg): cancellation tokens?
        public async Task HandleConnection(WebSocket webSocket)
        {
            var detectorAddress = await ReceiveMacAddress(webSocket);
            if (detectorAddress is null) return;

            var detector = await RegisterDetector(detectorAddress);

            await HandleCommandLoop(webSocket, detector);

            await DisconnectDetector(detector);
        }

        private async Task<PhysicalAddress?> ReceiveMacAddress(WebSocket webSocket)
        {
            var detectorAddressString = await ReceiveTextAsync(webSocket);
            try
            {
                return PhysicalAddress.Parse(detectorAddressString);
            }
            catch (FormatException _)
            {
                _logger.Warning("Connection attempt by detector with invalid MAC address: '{Mac}'", detectorAddressString);
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid MAC address",
                    CancellationToken.None);
                return null;
            }
        }

        private async Task DisconnectDetector(Detector detector)
        {
            _logger.Information("Detector (id: {Id}) disconnected", detector.Id);
            _queues.RemoveQueue(detector.Id);
            detector.State = DetectorState.Off;
            await _context.SaveChangesAsync();
        }

        private async Task HandleCommandLoop(WebSocket webSocket, Detector detector)
        {
            var queue = _queues.AddQueue(detector.Id);

            try
            {
                while (true)
                {
                    while (queue.Count == 0)
                    {
                        // TODO(rg): use BlockingConnection<T> or smth
                        queue.EnqueueEvent.WaitOne(_options.PingMilliseconds);
                        const DetectorCommandType ping = DetectorCommandType.Ping;
                        _logger.Debug("Sending '{Ping}' to detector (id: {Id})", ping, detector.Id);
                        var pingResult = await SendCommandWithVerificationAsync(webSocket, ping);
                        if (!pingResult)
                        {
                            // NOTE(rg): Detector didn't respond to ping, which means it's disconnected
                            return;
                        }
                    }

                    var command = queue.Dequeue();

                    _logger.Debug("Sending '{Cmd}' to detector (id: {Id})", command, detector.Id);
                    var result = await SendCommandWithVerificationAsync(webSocket, command);
                    if (!result) continue;

                    switch (command.Type)
                    {
                        case DetectorCommandType.StartStreaming:
                        {
                            detector.State = DetectorState.Running;
                            await _context.SaveChangesAsync();
                            break;
                        }
                        case DetectorCommandType.StopStreaming:
                        {
                            detector.State = DetectorState.Standby;
                            await _context.SaveChangesAsync();
                            break;
                        }
                        case DetectorCommandType.TakeSnapshot:
                        {
                            var snapshot = await ReceiveSnapshotAsync(webSocket);
                            _snapshotCache.Set(detector.Id, snapshot);
                            break;
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.Information("Lost connection to detector (id: {Id}). WebSocket error code: {@WsErr}",
                    detector.Id, ex.WebSocketErrorCode);
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
                    new CancellationTokenSource(_options.TimeoutMilliseconds).Token
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
            var image = new byte[_options.SnapshotBufferSize];
            var tempBuffer = new byte[_options.SnapshotBufferSize];
            var imageSize = 0;

            while (true)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(tempBuffer),
                        new CancellationTokenSource(_options.TimeoutMilliseconds).Token);

                    _logger.Warning("Result: {@Res}", result);
                    if (webSocket.State == WebSocketState.CloseReceived)
                    {
                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null,
                            new CancellationTokenSource(_options.TimeoutMilliseconds).Token);
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
            var buffer = new byte[_options.ResponseBufferSize];


            try
            {
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                    new CancellationTokenSource(_options.TimeoutMilliseconds).Token);

                if (webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null,
                        new CancellationTokenSource(_options.TimeoutMilliseconds).Token);
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


        private async Task<Detector> RegisterDetector(PhysicalAddress macAddress)
        {
            var existingDetector =
                await _context.Detectors.Where(d => d.MacAddress.Equals(macAddress)).SingleOrDefaultAsync();
            if (existingDetector is not null)
            {
                _logger.Information("Detector (id: {Id}) connected", existingDetector.Id);
                existingDetector.State = DetectorState.Standby;
                await _context.SaveChangesAsync();
                return existingDetector;
            }

            var detector = new Detector
            {
                Name = macAddress.ToString(),
                MacAddress = macAddress,
                State = DetectorState.Standby,
            };

            await _context.Detectors.AddAsync(detector);
            await _context.SaveChangesAsync();
            _logger.Information("Registered new detector with MAC address '{Mac}' (id: {Id})", macAddress, detector.Id);

            return detector;
        }
    }
}