using System.Runtime.InteropServices;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;

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

    public bool IsObjectBelongsTo(int id)
    {
        return Objects.Any() && Objects.Select(o => o.Id).Contains(id);
    }
}