using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class JobWithTasksSpec : Specification<Job>, ISingleResultSpecification
{
    public JobWithTasksSpec(int id)
    {
        Query.Where(j => j.Id == id)
            .Include(j => j.Tasks)
            .ThenInclude(t => t.Steps)
            .Include(j => j.Tasks)
            .ThenInclude(t => t.Objects);
    }
}