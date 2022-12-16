using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class LocationWithTasksSpec : Specification<Location>, ISingleResultSpecification
{
    public LocationWithTasksSpec(int id)
    {
        Query.Where(l => l.Id == id).Include(l => l.Tasks).ThenInclude(t => t.Instances);
    }
}