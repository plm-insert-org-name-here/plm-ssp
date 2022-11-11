using Domain.Common;
using FluentResults;

namespace Domain.Entities.TaskAggregate;

public class TaskInstance : IBaseEntity
{
    public int Id { get; private set; }
    public TaskInstanceFinalState? FinalState { get; private set; }
    public List<Event> Events { get; private set; } = default!;

    public Task Task { get; private set; } = default!;
    public int TaskId { get; private set; }
    public int[] RemainingStepIds { get; private set; } = default!;

    private TaskInstance()
    {
    }

    public TaskInstance(Task task)
    {
        Events = new List<Event>();
        RemainingStepIds = task.Steps.Select(s => s.Id).ToArray();
    }

    private void RemoveRemainingStep(int stepId)
    {
        var list = RemainingStepIds.ToList();
        list.Remove(stepId);
        RemainingStepIds = list.ToArray();
    }

    public Result AddEvent(Event ev)
    {
        if (!RemainingStepIds.Contains(ev.StepId))
            return Result.Fail("Current Task instance does not expect this Step");

        Events.Add(ev);

        RemoveRemainingStep(ev.StepId);

        if (IsEnded())
            FinalState = TaskInstanceFinalState.Completed;

        return Result.Ok();
    }

    public void Abandon()
    {
        FinalState = TaskInstanceFinalState.Abandoned;
    }

    public bool IsEnded()
    {
        return !RemainingStepIds.Any();
    }
}