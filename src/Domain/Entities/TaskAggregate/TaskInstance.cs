using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class TaskInstance : IBaseEntity
{
    public int Id { get; set; }
    public TaskInstanceFinalState? FinalState { get; set; }
    public List<Event> Events { get; set; } = default!;

    public Task Task { get; set; } = default!;
    public int TaskId { get; set; }
    public int[] Remaining { get; set; } = default!;

    private TaskInstance()
    {
    }

    public TaskInstance(Task task)
    {
        Events = new List<Event>();
        Remaining = task.Steps.Select(s => s.Id).ToArray();
    }

    public bool IsEnded(int stepId)
    {
        var list = Remaining.ToList();
        list.Remove(stepId);
        Remaining = list.ToArray();

        return !Remaining.Any();
    }
}