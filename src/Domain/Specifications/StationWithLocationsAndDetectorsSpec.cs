using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class StationWithLocationsAndDetectorsSpec : Specification<Station>, ISingleResultSpecification
{
    public StationWithLocationsAndDetectorsSpec(int id)
    {
        Query.Where(s => s.Id == id)
            .Include(s => s.Children)
            .ThenInclude(l => l.Detector);
    }
}