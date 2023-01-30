using Ardalis.Specification;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Specifications;

public sealed class TaskWithChildrenSpec : Specification<Task>
{
    public TaskWithChildrenSpec(int id)
    {
        Query.Where(t => t.Id == id)
            .Include(t => t.Objects)
            .Include(t => t.Steps)
            .Include(t => t.Job)
            .Include(t => t.Instances)
            .ThenInclude(i => i.Events)
            .AsSplitQuery();
    }
}