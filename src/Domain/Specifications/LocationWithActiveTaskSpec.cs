using Ardalis.Specification;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class LocationWithActiveTaskSpec : Specification<Location>
{
    public LocationWithActiveTaskSpec(int id)
    {
        Query.Where(l => l.Id == id)
            .Include(l => l.Detector)
            .Include(l => l.OngoingTask)
            .ThenInclude(t => t.Job)
            .Include(l => l.OngoingTask)
            .ThenInclude(t => t.Steps)
            .Include(l => l.OngoingTask)
            .ThenInclude(t => t.Objects)
            .Include(l => l.OngoingTask)
            .ThenInclude(t => t.OngoingInstance)
            .ThenInclude(ti => ti.Events);
    }
}