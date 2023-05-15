using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class CHNodeWithChildrenSpec<T, TChild> : Specification<T>
    where T: class, ICHNodeWithChildren<TChild>
    where TChild: class, ICHNode
{
    public CHNodeWithChildrenSpec(int id)
    {
        Query.Where(x => x.Id == id).Include(x => x.Children);
    }
}