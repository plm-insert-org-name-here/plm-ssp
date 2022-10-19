using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class StationWithLocationsSpec : Specification<Station>
{
    public StationWithLocationsSpec(int id)
    {
        Query.Where(s => s.Id == id).Include(s => s.Locations);
    }
}