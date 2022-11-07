using Domain.Common;

namespace Domain.Entities.TaskAggregate;

public class TaskInstance : IBaseEntity
{
    public int Id { get; set; }
    public TaskInstanceFinalState? FinalState { get; set; }
    public List<Event> Events { get; set; } = default!;

    public Task Task { get; set; } = default!;
    public int TaskId { get; set; }
    public int[] Remaining { get; set; }

    private TaskInstance()
    {
    }
    
    public bool IsEnded(int StepId)
    {
        var list = Remaining.ToList();
        list.Remove(StepId);
        Remaining = list.ToArray();
        
        if (!Remaining.Any())
        {
            return true;
        }
        return false;
    }
}