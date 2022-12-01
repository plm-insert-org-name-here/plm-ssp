using Ardalis.Specification;
using Domain.Entities.TaskAggregate;

namespace Domain.Specifications;

public sealed class LatestTaskInstanceByTaskIdWithEventsSpec : Specification<TaskInstance>, ISingleResultSpecification
{
    public LatestTaskInstanceByTaskIdWithEventsSpec(int id)
    {
        Query.Where(t => t.TaskId == id)
            .OrderByDescending(t => t.Id)
            .Include(t => t.Events)
            .ThenInclude(e => e.Step)
            .ThenInclude(s => s.Object)
            .Take(1);
    }
}