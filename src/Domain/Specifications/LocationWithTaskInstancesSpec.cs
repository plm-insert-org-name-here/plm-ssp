using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Specifications;

public sealed class LocationWithTaskInstancesSpec : Specification<Location>
{
    public LocationWithTaskInstancesSpec(int id)
    {
        Query.Where(l => l.Id == id)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Instances)
            .ThenInclude(i => i.Events).AsSplitQuery();
    }
}