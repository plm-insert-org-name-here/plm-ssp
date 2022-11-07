using Ardalis.Specification;
using Domain.Entities.TaskAggregate;

namespace Domain.Specifications;

public sealed class ActiveTaskInstanceWithEvents : Specification<TaskInstance>
{
    public ActiveTaskInstanceWithEvents(int id)
    {
        Query.Where(t => t.TaskId == id)
            .Where(t => t.FinalState == null)
            .Include(t => t.Events);
    }
}