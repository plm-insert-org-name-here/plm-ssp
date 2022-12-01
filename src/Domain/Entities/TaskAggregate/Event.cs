using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Event : IBaseEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }

    public EventResult Result { get; set; } = default!;

    public Step Step { get; set; } = default!;
    public int StepId { get; set; }

    public TaskInstance TaskInstance { get; set; } = default!;
    public int TaskInstanceId { get; set; }

    private Event() { }

    private Event(int id, DateTime timestamp, bool success, string? failureReason, int stepId, int taskInstanceId)
    {
        Id = id;
        Timestamp = timestamp;
        Result = EventResult.Create(success, failureReason).Value;
        StepId = stepId;
        TaskInstanceId = taskInstanceId;
    }

    public Event(DateTime timestamp, EventResult eventResult, int stepId, int taskInstanceId)
    {
        Timestamp = timestamp;
        Result = eventResult;
        StepId = stepId;
        TaskInstanceId = taskInstanceId;
    }
}