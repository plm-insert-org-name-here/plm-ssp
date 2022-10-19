using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class SiteWithOPUsSpec : Specification<Site>
{
    public SiteWithOPUsSpec(int id)
    {
        Query.Where(s => s.Id == id).Include(s => s.OPUs);
    }
}