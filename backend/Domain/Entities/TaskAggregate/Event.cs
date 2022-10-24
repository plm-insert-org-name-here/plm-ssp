using System;
using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Event : BaseEntity
{
    public DateTime Timestamp { get; set; }
    public EventResult Result { get; set; }

    public Step Step { get; set; } = default!;
    public int StepId { get; set; }
    // public string? FailureReason { get; set; }
    // public Step? StateChange { get; set; }
    // public int StateChangeId { get;set; }
    // public TaskInstance TaskInstance { get; set; } = default!;
    // public int TaskInstanceId { get; set; }
}