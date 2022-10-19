using System.Collections.Generic;
using TaskStatus = Domain.Common.TaskStatus;

namespace Domain.Entities.TaskAggregate;

public class Task : BaseEntity
{
    public string Name { get; set; } = default!;
    public TaskStatus Status { get; set; }
    public bool? Ordered { get; set; }
    public List<Template>? Templates { get; set; }
    public List<TaskInstance> TaskInstances { get; set; } = default!;

    public Job Job { get; set; } = default!;
    public int JobId { get; set; }
}