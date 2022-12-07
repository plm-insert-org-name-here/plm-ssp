using Ardalis.Specification;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Specifications;

public sealed class TaskWithOngoingInstanceSpec : Specification<Task>, ISingleResultSpecification
{
    public TaskWithOngoingInstanceSpec(int id)
    {
        Query.Where(t => t.Id == id)
            .Include(t => t.OngoingInstance)
            .ThenInclude(t => t.Events)
            .ThenInclude(e => e.Step)
            .ThenInclude(s => s.Object);
    }
}