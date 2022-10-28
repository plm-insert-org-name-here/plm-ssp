using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;

namespace Domain.Services;

public class CHNameUniquenessChecker<TParent, T> : ICHNameUniquenessChecker<TParent, T>
    where TParent: class, ICHNodeWithChildren<T>
    where T: class, ICHNode
{
    private readonly IRepository<TParent> _repo;

    public CHNameUniquenessChecker(IRepository<TParent> repo)
    {
        _repo = repo;
    }

    public async Task<bool> IsDuplicate(TParent parentNode, string name, T? existingNode)
    {
        return await _repo.AnyAsync(new CHNameUniquenessSpec<TParent, T>(parentNode, name, existingNode));
    }
}