using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Api.Services.ProcessorHandler
{
    public class PacketSender
    {
        private readonly ILogger _logger;
        private readonly ProcessorSocket _processorSocket;

        public PacketSender(
            ILogger logger,
            ProcessorSocket processorSocket)
        {
            _logger = logger;
            _processorSocket = processorSocket;
        }

        public async Task SendParameters(ParamsPacket ps)
        {
            // TODO(rg): cancellation token
            using (await _processorSocket.SockLock.Lock(CancellationToken.None))
            {
                _processorSocket.RemoteSocket ??=
                    await _processorSocket.ServerSocket.AcceptAsync();

                var mTypeBytes = BitConverter.GetBytes((int)PacketType.Params);
                var bytes = ps.ToBytes();

                await _processorSocket.RemoteSocket.SendAsync(mTypeBytes, SocketFlags.None);
                await _processorSocket.RemoteSocket.SendAsync(bytes, SocketFlags.None);
            }
        }

        public async Task SendFrame(FramePacket frame)
        {
            // TODO(rg): cancellation token
            using (await _processorSocket.SockLock.Lock(CancellationToken.None))
            {
                _processorSocket.RemoteSocket ??=
                    await _processorSocket.ServerSocket.AcceptAsync();

                var mTypeBytes = BitConverter.GetBytes((int)PacketType.Frame);
                var reqBytes = frame.ToBytes();

                await _processorSocket.RemoteSocket.SendAsync(mTypeBytes, SocketFlags.None);
                await _processorSocket.RemoteSocket.SendAsync(reqBytes, SocketFlags.None);
            }
        }
    }
}