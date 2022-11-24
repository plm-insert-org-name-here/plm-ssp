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

    public Event(DateTime timestamp, EventResult eventResult, int stepId, int taskInstanceId)
    {
        Timestamp = timestamp;
        Result = eventResult;
        StepId = stepId;
        TaskInstanceId = taskInstanceId;
    }
}