using Domain.Entities;

namespace Domain.Interfaces;

public interface IJobNameUniquenessChecker
{
    public Task<bool> IsDuplicate(string name, Job? existingJob);
}