using Ardalis.Specification;
using Domain.Entities.TaskAggregate;

namespace Domain.Specifications;

public sealed class TaskInstanceWithEvents : Specification<TaskInstance>
{
    public TaskInstanceWithEvents(int id)
    {
        Query.Where(t => t.TaskId == id)
            .Include(t => t.Events);
    }
}