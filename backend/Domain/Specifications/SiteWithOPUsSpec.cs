using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class SiteWithOPUsSpec : Specification<Site>
{
    public SiteWithOPUsSpec(int id)
    {
        Query.Where(s => s.Id == id).Include(s => s.Children);
    }
}