using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class CHNameUniquenessSpec<TParent, T> : Specification<TParent, List<T>>
    where TParent: class, ICompanyHierarchyNodeWithChildren<T>
    where T: class, ICompanyHierarchyNode
{
    public CHNameUniquenessSpec(TParent parentNode, string name, T? node)
    {
        Query
            .Where(x => x.Id == parentNode.Id)
            .Include(x => x.Children);

        Query.Select(x => x.Children);
        Query.
    }
}