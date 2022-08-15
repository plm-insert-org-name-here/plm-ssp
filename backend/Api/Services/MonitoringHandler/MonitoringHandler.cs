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
                await _sender.SendPacket(new PausePacket(task.Id));
                task.Status = TaskStatus.Paused;
            }
            else
            {
                await _sender.SendPacket(new StopPacket(task.Id));
                task.Status = TaskStatus.Inactive;
            }
        }

        private void SubmitEvent(TaskInstance ins, StateChange change, string? failureReason = null)
        {
            var ev = new Event
            {
                Timestamp = DateTime.Now,
                StateChange = change,
                TaskInstance = ins,
                Result = failureReason is null ? EventResult.Success : EventResult.Failure,
                FailureReason = failureReason
            };
            ins.Events.Add(ev);
        }

        private bool HandleTemplate(
            StateChange stateChange,
            TemplateState newState,
            TemplateStateHistory history,
            TaskInstance ins)
        {
            var ret = false;

            if (newState is TemplateState.UnknownObject &&
                history.PreviousState is not TemplateState.UnknownObject)
                SubmitEvent(ins, stateChange, $"Detected an unknown object at template (name: '${stateChange.Template.Name}')");

            if (history.PreviousValidState is null)
            {
                if ((newState is TemplateState.Missing && stateChange.ExpectedInitialState is TemplateState.Present) ||
                    (newState is TemplateState.Present && stateChange.ExpectedInitialState is TemplateState.Missing))
                    SubmitEvent(ins, stateChange, $"Incorrect initial state for template (name: '${stateChange.Template.Name}')");

                UpdateTemplateStateHistory(history, newState);
                return false;
            }

            if (history.PreviousValidState == stateChange.ExpectedInitialState
                && newState == stateChange.ExpectedSubsequentState)
            {
                SubmitEvent(ins, stateChange);
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
                var stateChange = task.Templates!
                    .SelectMany(t => t.StateChanges)
                    .SingleOrDefault(c => c.Id == id);
                if (stateChange is null)
                {
                    _logger.Error("Result packet contains invalid state change id {Id}", id);
                    return;
                }

                // Since the state change order nums are a linear series starting from 1 with a step of 1, we can
                // determine the next order num this way
                var currentOrderNum = ins.Events
                    .Where(e => e.Result == EventResult.Success)
                    .Select(e => e.StateChange!.OrderNum)
                    .Max()
                    .GetValueOrDefault(0) + 1;
                var currentStateChange = task.Templates!
                    .SelectMany(t => t.StateChanges)
                    .SingleOrDefault(c => c.OrderNum == currentOrderNum);
                if (currentStateChange is null)
                {
                    _logger.Error("Couldn't find state change with an OrderNum of {Ord}", currentOrderNum);
                    return;
                }

                var isLast = task.Templates!.Count == currentOrderNum;

                if (stateChange.Id != currentStateChange.Id)
                {
                    // TODO(rg): check for desync in unordered case (also refactor and deduplicate stuff)
                    // This is normal - when a new set of params is sent to the processor, we might receive a couple results
                    // for the previous set of params, until the processor updates the runner to use the new set
                    _logger.Debug(
                        "Next state change order is desynced between backend and processor; Actual: {Actual}, Result: {Result}",
                        currentStateChange.OrderNum, stateChange.OrderNum);
                    return;
                }

                var historyExists = TemplateStates.TryGetValue(stateChange.Template.Id, out var history);
                if (!historyExists)
                {
                    history = new TemplateStateHistory();
                    TemplateStates.Add(stateChange.Template.Id, history);
                }

                var success = HandleTemplate(stateChange, newState, history!, ins);

                if (success && isLast)
                {
                    ins.FinalState = TaskInstanceFinalState.Completed;
                    await StopMonitoring(det, task);
                }
                else if (success)
                {
                    // We're in !isLast, so this is guaranteed to return with a template
                    var nextTemplate = task.Templates!
                        .SelectMany(t => t.StateChanges)
                        .SingleOrDefault(c => c.OrderNum == currentOrderNum + 1);
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
            }
            else
            {
                var successIds = new List<int>(packet.TemplateStates.Count);
                foreach (var (id, newState) in packet.TemplateStates)
                {
                    var stateChange = task.Templates!
                        .SelectMany(t => t.StateChanges)
                        .SingleOrDefault(c => c.Id == id);
                    if (stateChange is null)
                    {
                        _logger.Error("Result packet contains invalid state change id {Id}", id);
                        return;
                    }

                    var historyExists = TemplateStates.TryGetValue(stateChange.Template.Id, out var history);
                    if (!historyExists)
                    {
                        history = new TemplateStateHistory();
                        TemplateStates.Add(stateChange.Template.Id, history);
                    }

                    var success = HandleTemplate(stateChange, newState, history!, ins);
                    if (success)
                        successIds.Add(id);
                }

                if (successIds.Count > 0)
                {
                    var remainingIds = packet.TemplateStates.Select(ts => ts.Item1).Except(successIds).ToArray();

                    if (remainingIds.Length == 0)
                    {
                        ins.FinalState = TaskInstanceFinalState.Completed;
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
                StateChange = null,
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
            ins.FinalState = TaskInstanceFinalState.Completed;

            await StopMonitoring(det, task);

            await context.SaveChangesAsync();
            await _sender.SendPacket(new StopPacket(packet.TaskId));
        }
    }
}