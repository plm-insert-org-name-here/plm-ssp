using Ardalis.Specification;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Specifications;

public sealed class TaskWithPrevInstances : Specification<Task>, ISingleResultSpecification
{
    public TaskWithPrevInstances(int id)
    {
        Query.Where(t => t.Id == id)
            .Include(t => t.Instances)
            .ThenInclude(i => i.Events);
    }
}