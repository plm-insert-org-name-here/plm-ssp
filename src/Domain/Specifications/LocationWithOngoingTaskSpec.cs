using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class LocationWithOngoingTaskSpec : Specification<Location>
{
    public LocationWithOngoingTaskSpec(int id)
    {
        Query.Where(l => l.Id == id)
            .Include(l => l.OngoingTask)
            .ThenInclude(t => t.OngoingInstance);
    }
}