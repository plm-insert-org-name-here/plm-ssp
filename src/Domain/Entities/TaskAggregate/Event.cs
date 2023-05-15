using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class Event : IBaseEntity
{
    public int Id { get; private set; }
    public DateTime Timestamp { get; private set; }

    public EventResult Result { get; private set; } = default!;

    public Step Step { get; private set; } = default!;
    public int StepId { get; private set; }

    public TaskInstance TaskInstance { get; private set; } = default!;
    public int TaskInstanceId { get; private set; }

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