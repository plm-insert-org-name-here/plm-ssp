using System;
using System.Collections.Generic;
using System.Linq;
using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Services.DetectorController;
using Api.Services.ProcessorHandler;
using Api.Services.ProcessorHandler.Packets.Req;
using Api.Services.ProcessorHandler.Packets.Res;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Api.Services.MonitoringHandler
{
    public class MonitoringHandler
    {
        private readonly StreamViewerGroups _groups;
        private readonly DetectorCommandQueues _queues;
        private readonly PacketSender _sender;
        private readonly ILogger _logger;

        // As long as templates are not shared between tasks, and only one instance of a task may run at a time,
        // this is fine
        private Dictionary<int, TemplateStateHistory> TemplateStates = new();

        public MonitoringHandler(
            StreamViewerGroups groups,
            DetectorCommandQueues queues,
            PacketSender sender,
            ILogger logger)
        {
            _groups = groups;
            _queues = queues;
            _sender = sender;
            _logger = logger;
        }

        public async Task StopMonitoring(Detector detector)
        {
            if (_groups.HasViewers(detector.Id))
                detector.State = DetectorState.Streaming;
            else
            {
                _queues.EnqueueCommand(detector.Id, DetectorCommandType.StopStreaming);
                detector.State = DetectorState.Standby;
            }

            await _sender.SendPacket(new StopPacket(detector.Id));
        }

        public async Task HandleKitResult(
            Context context,
            KitResultPacket packet,
            Domain.Entities.Task task,
            TaskInstance ins,
            Detector det)
        {
            foreach (var templ in packet.TemplateStates)
            {
                var template = task.Templates!.SingleOrDefault(t => t.Id == templ.Item1);
                var state = templ.Item2;

                if (template is null)
                {
                    _logger.Error("Template id {Id} doesn't exist within task '{TaskName}'!", templ.Item1, task.Name);
                    continue;
                }

                if (task.Ordered!.Value)
                {
                    // Since the template order nums are a linear series starting from 1 with a step of 1, we can
                    // determine the next order num this way
                    var nextOrderNum = ins.Events.Select(e => e.Template!.OrderNum).Max() + 1;
                    var nextTemplate = task.Templates!.Single(t => t.OrderNum == nextOrderNum);

                    if (template.OrderNum != nextTemplate.OrderNum)
                    {
                        _logger.Error("Template order numbers are desynchronized; Actual: {Actual}, Result: {Result}",
                            nextTemplate.OrderNum, template.OrderNum);
                    }

                    // TODO(rg): check state
                    // on failure: continue, Event(NOK)
                    // on success: go next, Event(OK)
                    // else: continue, Event(NOK) based on state

                    // on success, either:
                    // all templates are completed -> task is completed -> StopMonitoring & ins to Finished, task to Inactive
                    // next template -> send updated ParamsPacket


                }
                else
                {

                }
            }

            // TODO(rg): check for task completion
        }

        public async Task HandleQAResult(
            Context context,
            QAResultPacket packet,
            Domain.Entities.Task task,
            TaskInstance ins,
            Detector det)
        {
            if (packet.Result is QAResult.Uncertain)
                return;

            var ev = new Event
            {
                Timestamp = DateTime.Now,
                Template = null,
                TaskInstance = ins,
            };

            if (packet.Result is QAResult.Success)
            {
                ev.Result = EventResult.Success;
            }

            if (packet.Result is QAResult.Failure)
            {
                ev.Result = EventResult.Failure;
                ev.FailureReason = "Quality check has failed";
            }

            ins.Events.Add(ev);
            ins.Finished = true;

            await StopMonitoring(det);
            task.Status = TaskStatus.Inactive;

            await context.SaveChangesAsync();
            await _sender.SendPacket(new StopPacket(packet.DetectorId));
        }
    }
}