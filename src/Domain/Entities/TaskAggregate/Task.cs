using System.Runtime.InteropServices;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using FluentResults;

namespace Domain.Entities.TaskAggregate;

public class Task : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public TaskType Type { get; set; } = default!;
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
        State = TaskState.Inactive;
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
    }

    public Result StopCurrentInstance()
    {
        var currentInstance = Instances.FirstOrDefault(i => i.FinalState is null);
        if (currentInstance is null)
            return Result.Fail("Task does not have a running instance");

        if (State is not (TaskState.Active or TaskState.Paused))
            return Result.Fail("Task is Inactive, therefore it cannot be stopped");

        currentInstance.FinalState = TaskInstanceFinalState.Abandoned;
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