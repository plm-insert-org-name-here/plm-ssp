using System.Collections.Generic;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Entities;

public class Job : BaseEntity
{
    public string Name { get; set; } = default!;
    public List<Task> Tasks { get; set; } = default!;
}