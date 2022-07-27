using System.IO;
using System.Net.Sockets;
using Api.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Api.Services.ProcessorHandler
{
    public class ProcessorSocket
    {
        public Socket ServerSocket { get; }
        public Socket? RemoteSocket { get; set; }
        public CancellableLock SockLock { get; } = new();

        public ProcessorSocket(string socketPath)
        {
            ServerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            if (File.Exists(socketPath))
                File.Delete(socketPath);

            ServerSocket.Bind(new UnixDomainSocketEndPoint(socketPath));
            ServerSocket.Listen(1);
        }
    }
}