using Ardalis.Specification;
using Domain.Entities.TaskAggregate;

namespace Domain.Specifications;

public sealed class CurrentTaskInstanceWithEventsSpec : Specification<TaskInstance>
{
    public CurrentTaskInstanceWithEventsSpec(int id)
    {
        Query.Where(t => t.TaskId == id)
            .Where(t => t.FinalState == null)
            .Include(t => t.Events);
    }
}