using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;

namespace Domain.Services;

public class CHNameUniquenessChecker<TParent, T> : ICHNameUniquenessChecker<TParent, T>
    where TParent: class, ICompanyHierarchyNodeWithChildren<T>
    where T: class, ICompanyHierarchyNode
{
    private readonly IRepository<TParent> _repo;

    public CHNameUniquenessChecker(IRepository<TParent> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Check(TParent parentNode, string name, T? existingNode)
    {
        var parentWithChildren = await _repo.ListAsync(new CHNameUniquenessSpec<TParent, T>(parentNode, name, existingNode));
        var children = parentWithChildren.SelectMany(p => p.Children);
    }
}