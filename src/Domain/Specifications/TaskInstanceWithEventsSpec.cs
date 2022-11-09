using Ardalis.Specification;
using Domain.Entities.TaskAggregate;

namespace Domain.Specifications;

public sealed class TaskInstanceWithEventsSpec : Specification<TaskInstance>
{
    public TaskInstanceWithEventsSpec(int id)
    {
        Query.Where(t => t.TaskId == id)
            .Include(t => t.Events);
    }
}