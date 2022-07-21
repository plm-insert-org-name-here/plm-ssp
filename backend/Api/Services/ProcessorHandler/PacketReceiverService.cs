using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Api.Services.ProcessorHandler
{
    public class PacketReceiverService : BackgroundService
    {
        private readonly ProcessorSocket _processorSocket;
        private readonly ProcessorHandlerOpt _opt;
        private readonly ILogger _logger;

        public PacketReceiverService(
            ProcessorSocket processorSocket,
            IOptions<ProcessorHandlerOpt> opt,
            ILogger logger)
        {
            _processorSocket = processorSocket;
            _opt = opt.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunReceiver(stoppingToken);
        }

        public async Task<ResultPacket> ReceiveResult()
        {
            using (await _processorSocket.SockLock.Lock(CancellationToken.None))
            {
                _processorSocket.RemoteSocket ??=
                    await _processorSocket.ServerSocket.AcceptAsync();

                // TODO(rg): buffer size from config
                var buffer = new byte[1024];
                await _processorSocket.RemoteSocket.ReceiveAsync(buffer, SocketFlags.None);

                // TODO(rg): error handling (buffer content may be invalid)
                return ResultPacket.FromBytes(buffer);
            }
        }

        // TODO(rg): implement
        // for each template state, query from DB:
        // template id -> Template -> Task -> Job
        // (in the future, consider using an in memory cache to avoid DB roundtrip on every template in each result)

        // when a Task is started, create a new TaskResult instance; Events related to templates belonging to that task
        // should belong to that TaskResult instance

        // the exact semantics of an Event, and what triggers them, depend on the type of task they're associated with
        // ToolKit/ItemKit:
        //  - semantics: the state of the physical object associated with the template has changed
        //  - trigger: when the state changes
        // QA:
        //  - semantics: the physical object associated with the template is in some state
        //  - trigger: immediately (when the task is launched)

        // Events should probably have a "state" / "result" property
        // e.g. ItemKit:
        // 1. expected action: Present -> Missing, initial state: Present, state after: Missing => Event(OK)
        // 2. expected action: Present -> Missing, initial state: Missing => Event(NOK)
        // 3. expected action: Present -> Missing, initial state: Present, state after: Obstructed (e.g. hand) => no event
        // e.g. ToolKit:
        // 3. expected action: Missing -> Present, initial state: Missing, state after: UnknownObject => Event(NOK)
        // e.g. QA:
        // 1. expected state: OK, actual state: OK => Event(OK)
        // 2. expected state: OK, actual state: NOK => Event(NOK)

        // TaskResult FinalState should depend on the states of Events belonging to it
        private async Task ProcessResult(ResultPacket result)
        {
            throw new NotImplementedException();
        }


        private async Task RunReceiver(CancellationToken stoppingToken)
        {
            // TODO(rg): run continuously, read ResultPackets from processor socket,
            // and perform business logic
            while (true)
            {
                // NOTE(rg): not sure if needed
                // Pause for some amount of time to let PacketSender acquire the lock
                await Task.Delay(TimeSpan.FromMilliseconds(_opt.ReceiverPauseMilliseconds));

                var result = await ReceiveResult();
                await ProcessResult(result);
            }
        }
    }
}