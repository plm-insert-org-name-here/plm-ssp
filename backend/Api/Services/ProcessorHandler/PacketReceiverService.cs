using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Infrastructure.Database;
using Api.Services.ProcessorHandler.Packets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Task = System.Threading.Tasks.Task;
using TaskStatus = Api.Domain.Common.TaskStatus;

namespace Api.Services.ProcessorHandler
{
    public class PacketReceiverService : BackgroundService
    {
        private readonly ProcessorSocket _processorSocket;
        private readonly IDbContextFactory<Context> _contextFactory;
        private readonly ProcessorHandlerOpt _opt;
        private readonly ILogger _logger;

        public PacketReceiverService(
            IOptions<ProcessorHandlerOpt> opt,
            ILogger logger,
            IDbContextFactory<Context> contextFactory)
        {
            _processorSocket = new ProcessorSocket(opt.Value.ResSocketPath);
            _opt = opt.Value;
            _logger = logger;
            _contextFactory = contextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunReceiver(stoppingToken);
        }

        public async Task<ResultPacketBase?> ReceiveResult()
        {
            using (await _processorSocket.SockLock.Lock(CancellationToken.None))
            {
                _processorSocket.RemoteSocket ??=
                    await _processorSocket.ServerSocket.AcceptAsync();

                // Check whether there's data available to read. The method returns with true if the remote end closed the connection,
                // even if there is nothing to read, so we need to check for that later
                var canRead = _processorSocket.RemoteSocket.Poll(0, SelectMode.SelectRead);
                if (!canRead)
                {
                    return null;
                }

                var baseBuffer = new byte[12];
                var read_bytes = await _processorSocket.RemoteSocket.ReceiveAsync(baseBuffer, SocketFlags.None);
                if (read_bytes == 0)
                {
                    // Connection was closed by the remote end
                    // TODO(rg): consider doing this check on each Receive call - in case the remote dies while sending a packet
                    // (thus sending an incomplete packet), the backend might end up parsing it into a valid packet,
                    // which is wrong
                    return null;
                }
                var detectorId = BitConverter.ToInt32(baseBuffer, 0);
                var taskId = BitConverter.ToInt32(baseBuffer, 4);
                var jobType = (JobType)BitConverter.ToInt32(baseBuffer, 8);

                if (jobType is JobType.QA)
                {
                    var qaBuffer = new byte[4];
                    await _processorSocket.RemoteSocket.ReceiveAsync(qaBuffer, SocketFlags.None);
                    var result = (QAResult)BitConverter.ToInt32(qaBuffer, 0);

                    return new QAResultPacket
                    {
                        DetectorId = detectorId,
                        TaskId = taskId,
                        JobType = jobType,
                        Result = result
                    };
                }
                else
                {
                    var templatesLenBuffer = new byte[4];
                    await _processorSocket.RemoteSocket.ReceiveAsync(templatesLenBuffer, SocketFlags.None);
                    var templatesLen = BitConverter.ToInt32(templatesLenBuffer, 0);

                    var templateStates = new List<(int, TemplateState)>();
                    var templateStatesBuffer = new byte[8 * templatesLen];
                    await _processorSocket.RemoteSocket.ReceiveAsync(templateStatesBuffer, SocketFlags.None);
                    for (var i = 0; i < templatesLen; i++)
                    {
                        var id = BitConverter.ToInt32(templateStatesBuffer, 8 * i);
                        var state = (TemplateState)BitConverter.ToInt32(templateStatesBuffer, 8 * i + 4);
                        templateStates.Add((id, state));
                    }

                    return new KitResultPacket
                    {
                        DetectorId = detectorId,
                        TaskId = taskId,
                        JobType = jobType,
                        TemplateStates = templateStates
                    };
                }
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
        private async Task ProcessResult(ResultPacketBase result)
        {
            await using var context = _contextFactory.CreateDbContext();

            var task = await context.Tasks
                .SingleOrDefaultAsync(t => t.Id == result.TaskId);

            if (task is not null)
            {
                _logger.Warning("Task id: {Id}, name: {Name}", task.Id, task.Name);
            }
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
                if (result is not null)
                    await ProcessResult(result);
            }
        }
    }
}