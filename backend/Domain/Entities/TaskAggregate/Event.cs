using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Event : IBaseEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }

    public EventResult Result { get; set; } = default!;
    public string? FailureReason { get; set; }
    
    public Step Step { get; set; } = default!;
    public int StepId { get; set; }
    
    public TaskInstance TaskInstance { get; set; } = default!;
    public int TaskInstanceId { get; set; }

    private Event() { }
}