using System.Collections.Generic;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using TaskStatus = Domain.Common.TaskStatus;

namespace Domain.Entities.TaskAggregate;

public class Task : BaseEntity
{
    public string Name { get; set; } = default!;
    public TaskType Type { get; set; } = default!;
    public Location Location { get; set; } = default!;
    public int LocationId { get; set; }
    public List<TaskInstance> Instances { get; set; } = default!;

    public List<Object> Objects { get; set; } = default!;
    public List<Step> Steps { get; set; } = default!;
}