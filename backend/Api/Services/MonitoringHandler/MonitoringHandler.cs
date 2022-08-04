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
using AutoMapper;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace Api.Services.MonitoringHandler
{
    using TemplateId = Int32;

    // TODO(rg): error checking (e.g. SingleOrDefault instead of Single)
    // TODO(rg): better failureReason for Failure events
    public class MonitoringHandler
    {
        private readonly StreamViewerGroups _groups;
        private readonly DetectorCommandQueues _queues;
        private readonly PacketSender _sender;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        // As long as templates are not shared between tasks, and only one instance of a task may run at a time,
        // this is fine
        private Dictionary<TemplateId, TemplateStateHistory> TemplateStates = new();

        public MonitoringHandler(
            StreamViewerGroups groups,
            DetectorCommandQueues queues,
            PacketSender sender,
            ILogger logger,
            IMapper mapper)
        {
            _groups = groups;
            _queues = queues;
            _sender = sender;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task StopMonitoring(Detector detector, Domain.Entities.Task task, bool pause = false)
        {
            if (_groups.HasViewers(detector.Id))
                detector.State = DetectorState.Streaming;
            else
            {
                _queues.EnqueueCommand(detector.Id, DetectorCommandType.StopStreaming);
                detector.State = DetectorState.Standby;
            }

            if (pause)
            {
                await _sender.SendPacket(new PausePacket(detector.Id));
                task.Status = TaskStatus.Paused;
            }
            else
            {
                await _sender.SendPacket(new StopPacket(detector.Id));
                task.Status = TaskStatus.Inactive;
            }
        }

        private void SubmitEvent(TaskInstance ins, Template template, string? failureReason = null)
        {
            var ev = new Event
            {
                Timestamp = DateTime.Now,
                Template = template,
                TaskInstance = ins,
                Result = failureReason is null ? EventResult.Success : EventResult.Failure,
                FailureReason = failureReason
            };
            ins.Events.Add(ev);
        }

        private bool HandleTemplate(
            Template template,
            TemplateState newState,
            TemplateStateHistory history,
            TaskInstance ins)
        {
            var ret = false;

            if (newState is TemplateState.UnknownObject &&
                history.PreviousState is not TemplateState.UnknownObject)
                SubmitEvent(ins, template, $"Detected an unknown object at template (name: '${template.Name}')");

            if (history.PreviousValidState is null)
            {
                if ((newState is TemplateState.Missing && template.ExpectedInitialState is TemplateState.Present) ||
                    (newState is TemplateState.Present && template.ExpectedInitialState is TemplateState.Missing))
                    SubmitEvent(ins, template, $"Incorrect initial state for template (name: '${template.Name}')");

                UpdateTemplateStateHistory(history, newState);
                return false;
            }

            if (history.PreviousValidState == template.ExpectedInitialState
                && newState == template.ExpectedSubsequentState)
            {
                SubmitEvent(ins, template);
                ret = true;
            }

            UpdateTemplateStateHistory(history, newState);

            return ret;
        }

        private void UpdateTemplateStateHistory(TemplateStateHistory history, TemplateState newState)
        {
            history.PreviousState = newState;
            if (newState.IsValid())
                history.PreviousValidState = newState;
        }

        public async Task HandleKitResult(
            Context context,
            KitResultPacket packet,
            Domain.Entities.Task task,
            TaskInstance ins,
            Detector det)
        {
            if (task.Ordered!.Value)
            {
                if (packet.TemplateStates.Count != 1)
                {
                    _logger.Error("Result packet for ordered kit contains more than 1 template state");
                    return;
                }

                var (id, newState) = packet.TemplateStates[0];
                var template = task.Templates!.Single(t => t.Id == id);

                // Since the template order nums are a linear series starting from 1 with a step of 1, we can
                // determine the next order num this way
                var currentOrderNum = ins.Events
                    .Where(e => e.Result == EventResult.Success)
                    .Select(e => e.Template!.OrderNum)
                    .Max()
                    .GetValueOrDefault(0) + 1;
                var currentTemplate = task.Templates!.SingleOrDefault(t => t.OrderNum == currentOrderNum);
                if (currentTemplate is null)
                {
                    _logger.Error("Couldn't find template with an OrderNum of {Ord}", currentOrderNum);
                    return;
                }

                var isLast = task.Templates!.Count == currentOrderNum;

                if (template.Id != currentTemplate.Id)
                {
                    _logger.Error(
                        "Next template order is desynced between backend and processor; Actual: {Actual}, Result: {Result}",
                        currentTemplate.OrderNum, template.OrderNum);
                }

                var historyExists = TemplateStates.TryGetValue(template.Id, out var history);
                if (!historyExists)
                {
                    history = new TemplateStateHistory();
                    TemplateStates.Add(template.Id, history);
                }

                var success = HandleTemplate(template, newState, history!, ins);
                _logger.Warning("HandleTemplate done: {Succ}", success);

                if (success && isLast)
                {
                    await StopMonitoring(det, task);
                }
                else if (success)
                {
                    // We're in !isLast, so this is guaranteed to return with a template
                    var nextTemplate = task.Templates!.SingleOrDefault(t => t.OrderNum == currentOrderNum + 1);
                    if (nextTemplate is null)
                    {
                        _logger.Error("Couldn't find next template with an OrderNum of {Ord}", currentOrderNum + 1);
                        return;
                    }

                    try
                    {
                        var nextParams = new ParamsPacket
                        {
                            DetectorId = packet.DetectorId,
                            TaskId = packet.TaskId,
                            JobType = packet.JobType,
                            Templates = new List<ParamsPacketTemplate>
                            {
                                _mapper.Map<ParamsPacketTemplate>(nextTemplate)
                            }
                        };

                        await _sender.SendPacket(nextParams);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("{Ex}", ex);
                    }
                }

                _logger.Warning("Done with the result");
            }
            else
            {
                var successIds = new List<int>(packet.TemplateStates.Count);
                foreach (var (id, newState) in packet.TemplateStates)
                {
                    var template = task.Templates!.Single(t => t.Id == id);
                    var historyExists = TemplateStates.TryGetValue(template.Id, out var history);
                    if (!historyExists)
                    {
                        history = new TemplateStateHistory();
                        TemplateStates.Add(template.Id, history);
                    }

                    var success = HandleTemplate(template, newState, history!, ins);
                    if (success)
                        successIds.Add(id);
                }

                if (successIds.Count > 0)
                {
                    var remainingIds = packet.TemplateStates.Select(ts => ts.Item1).Except(successIds).ToArray();

                    if (remainingIds.Length == 0)
                    {
                        await StopMonitoring(det, task);
                    }

                    var remainingTemplates = task.Templates!.Where(t => remainingIds.Contains(t.Id)).ToArray();

                    var nextParams = new ParamsPacket
                    {
                        DetectorId = packet.DetectorId,
                        TaskId = packet.TaskId,
                        JobType = packet.JobType,
                        Templates = _mapper.Map<List<ParamsPacketTemplate>>(remainingTemplates)
                    };
                    await _sender.SendPacket(nextParams);
                }
            }

            await context.SaveChangesAsync();
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

            await StopMonitoring(det, task);

            await context.SaveChangesAsync();
            await _sender.SendPacket(new StopPacket(packet.DetectorId));
        }
    }
}