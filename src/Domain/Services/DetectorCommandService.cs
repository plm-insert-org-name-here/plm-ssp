using Domain.Common;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FluentResults;
using Microsoft.Extensions.Logging;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Services;

public class DetectorCommandService
{
    private readonly IDetectorConnection _detectorConnection;
    private readonly IRepository<Location> _locationRepo;
    private readonly IRepository<Task> _taskRepo;
    private readonly ILogger<DetectorCommandService> _logger;

    public DetectorCommandService(IDetectorConnection detectorConnection, IRepository<Task> taskRepo, IRepository<Location> locationRepo, ILogger<DetectorCommandService> logger)
    {
        _detectorConnection = detectorConnection;
        _taskRepo = taskRepo;
        _locationRepo = locationRepo;
        _logger = logger;
    }

    public async Task<Result> HandleCommand(Detector detector, DetectorCommand command, CancellationToken ct)
    {
        var location = detector.LocationId.HasValue ?
            await _locationRepo.FirstOrDefaultAsync(new LocationWithTasksSpec(detector.LocationId.Value), ct) :
            null;

        if (detector.State is DetectorState.Off)
            return Result.Fail("Detector is offline");

        if (location is null &&
            (command.IsStartDetection ||
             command.IsPauseDetection ||
             command.IsResumeDetection ||
             command.IsStopDetection))
            return Result.Fail("Detection commands can't be sent to detached detectors");

        if (location is not null)
        {
            if (command.IsStartDetection)
            {
                if (location.OngoingTask is not null)
                    return Result.Fail("A new instance of a Task cannot be started while another is in progress");

                var taskId = ((DetectorCommand.StartDetection)command).TaskId;
                var task = await _taskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(taskId), ct);
                if (task is null)
                    return Result.Fail("The specified Task does not exist");

                if (!location.Tasks.Contains(task))
                    return Result.Fail("The Detector cannot start the specified Task, as it does not belong to the Detector's Location");

                var createResult = task.CreateInstance();
                if (createResult.IsFailed) return createResult;

                location.OngoingTask = task;

                detector.RemoveFromState(DetectorState.Standby);
                detector.AddToState(DetectorState.Monitoring);
            }
            else if (command.IsStopDetection)
            {
                if (location.OngoingTask is null)
                    return Result.Fail("The Detector is not running a Task");

                var result = location.OngoingTask.StopCurrentInstance();
                if (result.IsFailed) return result;

                location.OngoingTask = null;

                detector.RemoveFromState(DetectorState.Monitoring);
                detector.AddToState(DetectorState.Standby);
            }
            else if (command.IsPauseDetection)
            {
                if (location.OngoingTask is null)
                    return Result.Fail("The Detector is not running a Task");

                var result = location.OngoingTask.PauseCurrentInstance();
                if (result.IsFailed) return result;

                detector.RemoveFromState(DetectorState.Monitoring);
                detector.AddToState(DetectorState.Standby);
            }
            else if (command.IsResumeDetection)
            {
                if (location.OngoingTask is null)
                    return Result.Fail("The Detector is not running a Task");

                var result = location.OngoingTask.ResumeCurrentInstance();
                if (result.IsFailed) return result;

                detector.RemoveFromState(DetectorState.Standby);
                detector.AddToState(DetectorState.Monitoring);
            }
        }

        return await _detectorConnection.SendCommand(detector, command);
    }
}