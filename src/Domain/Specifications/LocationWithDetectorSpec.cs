using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class LocationWithDetectorSpec : Specification<Location>
{
    public LocationWithDetectorSpec(int id)
    {
        Query.Where(l => l.Id == id).Include(l => l.Detector);
    }
}