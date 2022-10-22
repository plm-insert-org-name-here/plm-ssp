using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class JobWithTasksSpec : Specification<Job>
{
    public JobWithTasksSpec(int id)
    {
        Query.Where(j => j.Id == id).Include(job => job.Tasks);
    }
}