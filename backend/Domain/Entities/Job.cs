using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Entities;

public class Job : BaseEntity
{
    public string Name { get; set; } = default!;
    public JobType Type { get; set; }
    public byte[] Snapshot { get; set; } = default!;

    public Location Location { get; set; } = default!;
    public int LocationId { get; set; }

    public List<Task> Tasks { get; set; } = default!;
}