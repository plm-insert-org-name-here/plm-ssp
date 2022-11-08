using Domain.Common;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FluentResults;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Services;

public class DetectorCommandService
{
    private readonly IDetectorConnection _detectorConnection;
    private readonly IRepository<Location> _locationRepo;
    private readonly IRepository<Task> _taskRepo;

    public DetectorCommandService(IDetectorConnection detectorConnection, IRepository<Task> taskRepo, IRepository<Location> locationRepo)
    {
        _detectorConnection = detectorConnection;
        _taskRepo = taskRepo;
        _locationRepo = locationRepo;
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
            var ongoingTask = location.Tasks
                .FirstOrDefault(t => t.State is TaskState.Active or TaskState.Paused);

            if (command.IsStartDetection)
            {
                if (ongoingTask is not null)
                    return Result.Fail("A new instance of a Task cannot be started while another is in progress");

                var taskId = ((DetectorCommand.StartDetection)command).TaskId;
                var task = await _taskRepo.GetByIdAsync(taskId, ct);
                if (task is null)
                    return Result.Fail("The specified Task does not exist");

                if (location.Tasks.Contains(task))
                    return Result.Fail("The Detector cannot start the specified Task, as it does not belong to the Detector's Location");

                task.CreateInstance();
            }
            else if (command.IsStopDetection)
            {
                if (ongoingTask is null)
                    return Result.Fail("The Detector is not running a Task");

                var result = ongoingTask.StopCurrentInstance();
                if (result.IsFailed) return result;
            }
            else if (command.IsPauseDetection)
            {
                if (ongoingTask is null)
                    return Result.Fail("The Detector is not running a Task");

                var result = ongoingTask.PauseCurrentInstance();
                if (result.IsFailed) return result;
            }
            else if (command.IsResumeDetection)
            {
                if (ongoingTask is null)
                    return Result.Fail("The Detector is not running a Task");

                var result = ongoingTask.ResumeCurrentInstance();
                if (result.IsFailed) return result;
            }
        }

        await _detectorConnection.SendCommand(detector, command);

        return Result.Ok();
    }
}