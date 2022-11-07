using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class CHNameUniquenessSpec<TParent, T> : Specification<TParent>
    where TParent: class, ICHNodeWithChildren<T>
    where T: class, ICHNode
{
    public CHNameUniquenessSpec(TParent parentNode, string name, T? node)
    {
        Query
            .Where(x => x.Id == parentNode.Id)
            .Include(x => x.Children);

        if (node is null)
        {
            Query.Where(x => x.Children.Any(c => c.Name == name));
        }
        else
        {
            Query.Where(x => x.Children.Any(c => c.Id != node.Id && c.Name == name));
        }
    }
}