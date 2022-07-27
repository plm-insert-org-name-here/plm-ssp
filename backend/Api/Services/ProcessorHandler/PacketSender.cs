using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Api.Services.ProcessorHandler.Packets;
using Api.Services.ProcessorHandler.Packets.Req;
using Microsoft.Extensions.Options;
using Serilog;

namespace Api.Services.ProcessorHandler
{
    public class PacketSender
    {
        private readonly ILogger _logger;
        private readonly ProcessorSocket _processorSocket;

        public PacketSender(
            ILogger logger,
            IOptions<ProcessorHandlerOpt> opt)
        {
            _logger = logger;
            _processorSocket = new ProcessorSocket(opt.Value.ReqSocketPath);
        }

        public async Task SendPacket(IReqPacket packet)
        {
            // TODO(rg): cancellation token
            using (await _processorSocket.SockLock.Lock(CancellationToken.None))
            {
                _processorSocket.RemoteSocket ??=
                    await _processorSocket.ServerSocket.AcceptAsync();

                var mTypeBytes = BitConverter.GetBytes((int)packet.Type);
                var bytes = packet.ToBytes();

                await _processorSocket.RemoteSocket.SendAsync(mTypeBytes, SocketFlags.None);
                await _processorSocket.RemoteSocket.SendAsync(bytes, SocketFlags.None);
            }
        }
    }
}