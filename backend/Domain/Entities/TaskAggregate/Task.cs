using Domain.Common;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Entities.TaskAggregate;

public class Task : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public TaskType Type { get; set; } = default!;
    public Location? Location { get; set; } = default!;
    public int? LocationId { get; set; }
    public List<TaskInstance> Instances { get; set; } = default!;

    public List<Object> Objects { get; set; } = default!;
    public List<Step> Steps { get; set; } = default!;
    
    private Task() {}

    public Task(string name, List<Object> objects, List<Step> steps)
    {
        Name = name;
        Objects = objects;
        Steps = steps;
    }

    public bool IsObjectBelongsTo(int id)
    {
        return Objects.Select(o => o.Id).Contains(id);
    }
}