using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Application.Services.ProcessorHandler.Packets.Res;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Application.Services.ProcessorHandler
{
    public class PacketReceiverService : BackgroundService
    {
        private readonly ProcessorSocket _processorSocket;
        private readonly IDbContextFactory<Context> _contextFactory;
        private readonly ProcessorHandlerOpt _opt;
        private readonly ILogger _logger;
        private readonly PacketSender _sender;
        private readonly MonitoringHandler.MonitoringHandler _monitoringHandler;

        public PacketReceiverService(
            IOptions<ProcessorHandlerOpt> opt,
            ILogger logger,
            IDbContextFactory<Context> contextFactory,
            PacketSender sender,
            MonitoringHandler.MonitoringHandler monitoringHandler)
        {
            _processorSocket = new ProcessorSocket(opt.Value.ResSocketPath);
            _opt = opt.Value;
            _logger = logger;
            _contextFactory = contextFactory;
            _sender = sender;
            _monitoringHandler = monitoringHandler;
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
                // var canRead = _processorSocket.RemoteSocket.Poll(0, SelectMode.SelectRead);
                // if (!canRead)
                // {
                //     return null;
                // }

                var baseBuffer = new byte[12];
                var readBytes = await _processorSocket.RemoteSocket.ReceiveAsync(baseBuffer, SocketFlags.None);
                if (readBytes == 0)
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

        private async Task ProcessResult(ResultPacketBase packet)
        {
            await using var context = _contextFactory.CreateDbContext();

            // TODO(rg): benchmark and consider using an in-memory cache (these queries run on each result,
            // and they can't be modified while monitoring is active)
            var task = await context.Tasks
                .Include(t => t.Templates)
                .ThenInclude(t => t.StateChanges)
                .SingleOrDefaultAsync(t => t.Id == packet.TaskId);
            if (task is null)
            {
                _logger.Error("Task with id {Id} does not exist", packet.TaskId);
                return;
            }

            var taskInstance = await context.TaskInstances
                .Where(ti => ti.FinalState == null)
                .Include(ti => ti.Events)
                .ThenInclude(e => e.StateChange!)
                .ThenInclude(c => c.Template)
                .SingleOrDefaultAsync(ti => ti.Task.Id == packet.TaskId);
            if (taskInstance is null)
            {
                _logger.Error("No active task instance exists for task id {Id}", packet.TaskId);
                return;
            }

            var detector = await context.Detectors
                .SingleOrDefaultAsync(d => d.Id == packet.DetectorId);
            if (detector is null)
            {
                _logger.Error("Detector with id {Id} does not exist", packet.DetectorId);
                return;
            }

            if (packet is KitResultPacket kitResult)
                await _monitoringHandler.HandleKitResult(context, kitResult, task, taskInstance, detector);
            else if (packet is QAResultPacket qaResult)
                await _monitoringHandler.HandleQAResult(context, qaResult, task, taskInstance, detector);
        }

        private async Task RunReceiver(CancellationToken stoppingToken)
        {
            while (true)
            {
                var result = await ReceiveResult();
                if (result is not null)
                {
                    _logger.Warning("Result: {@Res}", result);
                    await ProcessResult(result);
                }
            }
        }
    }
}