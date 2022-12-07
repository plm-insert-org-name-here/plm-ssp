using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;

namespace Application.Services;

public class JobNameUniquenessChecker : IJobNameUniquenessChecker
{
    private readonly IRepository<Job> _repo;

    public JobNameUniquenessChecker(IRepository<Job> repo)
    {
        _repo = repo;
    }

    public async Task<bool> IsDuplicate(string name, Job? existingJob)
    {
        return await _repo.AnyAsync(new JobNameUniquenessSpec(name, existingJob));
    }
}