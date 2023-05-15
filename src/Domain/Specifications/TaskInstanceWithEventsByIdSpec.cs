using Ardalis.Specification;
using Domain.Entities.TaskAggregate;
using Microsoft.EntityFrameworkCore;

namespace Domain.Specifications;

public sealed class TaskInstanceWithEventsByIdSpec : Specification<TaskInstance>
{
    public TaskInstanceWithEventsByIdSpec(int id)
    {
        Query.Where(i => i.Id == id)
            .Include(i => i.Events)
            .ThenInclude(e => e.Step)
            .ThenInclude(s => s.Object);
    }
}