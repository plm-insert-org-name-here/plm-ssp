using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class JobNameUniquenessSpec : Specification<Job>
{
    public JobNameUniquenessSpec(string name, Job? existingJob)
    {
        if (existingJob is null)
        {
            Query.Where(j => j.Name == name);
        }
        else
        {
            Query.Where(j => j.Id != existingJob.Id && j.Name == name);
        }
    }
}