using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class SiteNameUniquenessSpec<T> : Specification<T>
    where T : ICHNode
{
    public SiteNameUniquenessSpec(string name, T? node)
    {
        if (node is null)
        {
            Query.Where(n => n.Name == name);
        }
        else
        {
            Query.Where(n => n.Id != node.Id && n.Name == name);
        }
    }
}