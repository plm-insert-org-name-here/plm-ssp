using System.IO;
using System.Net.Sockets;
using Application.Utils;

namespace Application.Services.ProcessorHandler
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