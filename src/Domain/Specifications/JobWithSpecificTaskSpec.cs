using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class JobWithSpecificTaskSpec : Specification<Job>, ISingleResultSpecification
{
    public JobWithSpecificTaskSpec(int id, int taskId)
    {
        Query.Where(j => j.Id == id)
            .Include(job => job.Tasks.Where(t => t.Id == taskId))
            .ThenInclude(t => t.Location)
            .Include(job => job.Tasks.Where(t => t.Id == taskId))
            .ThenInclude(t => t.Objects)
            .Include(j => j.Tasks.Where(t => t.Id == taskId))
            .ThenInclude(t => t.Steps);
    }
}