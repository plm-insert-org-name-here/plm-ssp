

using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

public class LocationWithDetectorAndChildrenSpec : Specification<Location>
{
    public LocationWithDetectorAndChildrenSpec(int id)
    {
        Query.Where(l => l.Id == id)
            .Include(l => l.Detector);
    }
}