using Ardalis.Specification;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Specifications;

public sealed class TaskWithChildrenSpec : Specification<Task>
{
    public TaskWithChildrenSpec(int id)
    {
        Query.Where(t => t.Id == id).Include(t => t.Templates).Include(t => t.TaskInstances);
    }
}