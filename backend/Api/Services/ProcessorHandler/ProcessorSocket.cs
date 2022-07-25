using System.IO;
using System.Net.Sockets;
using Api.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Api.Services.ProcessorHandler
{
    public class ProcessorSocket
    {
        private readonly ProcessorHandlerOpt _opt;

        public Socket ServerSocket { get; }
        public Socket? RemoteSocket { get; set; }
        public CancellableLock SockLock { get; } = new();

        public ProcessorSocket(IOptions<ProcessorHandlerOpt> opt)
        {
            _opt = opt.Value;

            ServerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            if (File.Exists(_opt.UnixSocketPath))
                File.Delete(_opt.UnixSocketPath);

            ServerSocket.Bind(new UnixDomainSocketEndPoint(_opt.UnixSocketPath));
            ServerSocket.Listen(1);
        }
    }
}