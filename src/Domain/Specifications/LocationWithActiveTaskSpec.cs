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
            .Include(l => l.Tasks.Where(t => t.State != TaskState.Inactive))
            .ThenInclude(t => t.Job)
            .Include(l => l.Tasks.Where(t => t.State != TaskState.Inactive))
            .ThenInclude(t => t.Steps)
            .Include(l => l.Tasks.Where(t => t.State != TaskState.Inactive))
            .ThenInclude(t => t.Objects)
            .Include(l => l.Tasks.Where(t => t.State != TaskState.Inactive))
            .ThenInclude(t => t.Instances.Where(ti => ti.FinalState == null))
            .ThenInclude(ti => ti.Events);
    }
}