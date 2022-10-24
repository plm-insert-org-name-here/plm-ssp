using System.Collections.Generic;
using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class TaskInstance : BaseEntity
{
    public TaskInstanceFinalState? FinalState { get; set; }
    public List<Event> Events { get; set; } = default!;

}