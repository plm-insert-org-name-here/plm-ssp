using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Api.Utils;
using LanguageExt.Pipes;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Api.Services.ProcessorHandler
{
    public class ProcessorHandler
    {
        private readonly ILogger _logger;
        private readonly ProcessorHandlerOpt _opt;
        private readonly CancellableLock _lock = new();

        private Socket ServerSocket { get; set; }
        private Socket? ProcessorSocket { get; set; }

        public ProcessorHandler(
            ILogger logger,
            IConfiguration configuration,
            ProcessorHandlerOpt opt)
        {
            _logger = logger;
            _opt = opt;

            configuration.GetSection(ProcessorHandlerOpt.SectionName).Bind(_opt);

            ServerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            if (File.Exists(_opt.UnixSocketPath))
                File.Delete(_opt.UnixSocketPath);

            ServerSocket.Bind(new UnixDomainSocketEndPoint(_opt.UnixSocketPath));
            ServerSocket.Listen(1);
        }

        public async Task SendParameters(ParamsPacket ps)
        {
            using (await _lock.Lock(CancellationToken.None))
            {
                ProcessorSocket ??= await ServerSocket.AcceptAsync();

                var mTypeBytes = BitConverter.GetBytes((int)PacketType.Params);
                var bytes = ps.ToBytes();

                await ProcessorSocket.SendAsync(mTypeBytes, SocketFlags.None);
                await ProcessorSocket.SendAsync(bytes, SocketFlags.None);
            }
        }

        public async Task SendFrame(FramePacket frame)
        {
            using (await _lock.Lock(CancellationToken.None))
            {
                // TODO(rg): cancellation token
                ProcessorSocket ??= await ServerSocket.AcceptAsync();

                var mTypeBytes = BitConverter.GetBytes((int)PacketType.Frame);
                var reqBytes = frame.ToBytes();

                await ProcessorSocket.SendAsync(mTypeBytes, SocketFlags.None);
                await ProcessorSocket.SendAsync(reqBytes, SocketFlags.None);
            }
        }

        public async Task<ResultPacket?> TryReadResponse()
        {
            using (await _lock.Lock(CancellationToken.None))
            {
                // TODO(rg): cancellation token
                ProcessorSocket ??= await ServerSocket.AcceptAsync();

                throw new NotImplementedException();
            }
        }
    }
}