using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class CHNodeWithParentSpec<TParent, T> : Specification<T>
    where T: class, ICHNodeWithParent<TParent>
    where TParent: class, ICHNode
{
    public CHNodeWithParentSpec(int id)
    {
        Query.Where(x => x.Id == id).Include(x => x.Parent);
    }
}