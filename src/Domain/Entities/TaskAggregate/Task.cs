using System.Runtime.InteropServices;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using FluentResults;

namespace Domain.Entities.TaskAggregate;

public class Task : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; private set; } = default!;
    public TaskType Type { get; set; }
    public TaskState State { get; set; }

    public int LocationId { get; set; }
    public int JobId { get; set; }

    public Location Location { get; set; } = default!;
    public List<TaskInstance> Instances { get; set; } = default!;
    public List<Object> Objects { get; set; } = default!;
    public List<Step> Steps { get; set; } = default!;

    private Task() {}

    private Task(int id, string name, TaskType type, int locationId, int jobId, TaskState taskState)
    {
        Id = id;
        Name = name;
        Type = type;
        LocationId = locationId;
        JobId = jobId;
        Instances = new List<TaskInstance>();
        Objects = new List<Object>();
        Steps = new List<Step>();
        State = taskState;
    }

    public Task(string name, List<Object> objects, List<Step> steps, int locationId)
    {
        Name = name;
        Objects = objects;
        Steps = steps;
        LocationId = locationId;
    }

    public void CreateInstance()
    {
        var instance = new TaskInstance(this);

        Instances.Add(instance);
        State = TaskState.Active;
    }

    public void AddObjects(IEnumerable<Object> objects)
    {
        Objects.AddRange(objects);
    }

    public void AddSteps(IEnumerable<Step> steps)
    {
        Steps.AddRange(steps);
    }

    public Result ModifyObject(int objectId, string name, ObjectCoordinates coords)
    {
        var obj = Objects.FirstOrDefault(o => o.Id == objectId);
        if (obj is null)
            return Result.Fail("Object to modify does not exist within the Task");

        var renameResult = obj.Rename(name, this);
        if (renameResult.IsFailed) return renameResult;

        var coordsResult = obj.UpdateCoordinates(coords);
        if (coordsResult.IsFailed) return coordsResult;

        return Result.Ok();
    }

    public Result ModifyStep(int stepId, int orderNum, TemplateState exInitState, TemplateState exSubsState, Object obj)
    {
        var step = Steps.FirstOrDefault(s => s.Id == stepId);
        if (step is null)
            return Result.Fail("Step to modify does not exist within the Task");

        step.OrderNum = orderNum;
        step.ExpectedInitialState = exInitState;
        step.ExpectedSubsequentState = exSubsState;
        step.Object = obj;

        return Result.Ok();
    }

    public void RemoveObjects(IEnumerable<int> objectIds)
    {
        Objects.RemoveAll(o => objectIds.Contains(o.Id));
    }

    public void RemoveSteps(IEnumerable<int> stepIds)
    {
        Steps.RemoveAll(s => stepIds.Contains(s.Id));
    }

    public Result Rename(string newName, Job parentJob)
    {
        if (parentJob.Tasks.Where(t => t.Id != Id).Select(t => t.Name).Contains(newName))
            return Result.Fail("Tasks' names within the same Job must be unique");

        Name = newName;

        return Result.Ok();
    }

    public Result AddEventToCurrentInstance(int stepId, EventResult eventResult, Detector detector)
    {
        if (State is not TaskState.Active)
            return Result.Fail("Task is not active");

        var currentInstance = Instances.FirstOrDefault(i => i.FinalState is null);
        if (currentInstance is null)
            return Result.Fail("Task does not have a running instance");

        if (!Steps.Select(s => s.Id).Contains(stepId))
            return Result.Fail("Task does not contain the referenced Step");

        var addEventResult = currentInstance.AddEvent(new Event(DateTime.Now, eventResult, stepId, currentInstance.Id));
        if (addEventResult.IsFailed) return addEventResult;

        if (currentInstance.IsEnded())
        {
            State = TaskState.Inactive;
            detector.RemoveFromState(DetectorState.Monitoring);
            detector.AddToState(DetectorState.Standby);
        }

        return Result.Ok();
    }

    public Result StopCurrentInstance()
    {
        var currentInstance = Instances.FirstOrDefault(i => i.FinalState is null);
        if (currentInstance is null)
            return Result.Fail("Task does not have a running instance");

        if (State is not (TaskState.Active or TaskState.Paused))
            return Result.Fail("Task is Inactive, therefore it cannot be stopped");

        currentInstance.Abandon();
        State = TaskState.Inactive;

        return Result.Ok();
    }

    public Result PauseCurrentInstance()
    {
        var currentInstance = Instances.FirstOrDefault(i => i.FinalState is null);
        if (currentInstance is null)
            return Result.Fail("Task does not have a running instance");

        if (State is not TaskState.Active)
            return Result.Fail("Task is not Active, therefore it cannot be paused");

        State = TaskState.Paused;

        return Result.Ok();
    }

    public Result ResumeCurrentInstance()
    {
        var currentInstance = Instances.FirstOrDefault(i => i.FinalState is null);
        if (currentInstance is null)
            return Result.Fail("Task does not have a running instance");

        if (State is not TaskState.Paused)
            return Result.Fail("Task is not Paused, therefore it cannot be resumed");

        State = TaskState.Active;

        return Result.Ok();
    }

    public bool IsObjectBelongsTo(int id)
    {
        return Objects.Any() && Objects.Select(o => o.Id).Contains(id);
    }
}