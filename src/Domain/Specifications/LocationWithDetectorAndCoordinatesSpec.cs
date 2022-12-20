using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class LocationWithDetectorAndCoordinatesSpec : Specification<Location>, ISingleResultSpecification
{
    public LocationWithDetectorAndCoordinatesSpec(int id)
    {
        Query.Where(l => l.Id == id)
            .Include(l => l.Detector)
            .Include(l => l.Coordinates)
            .ThenInclude(c => c!.Qr)
            .Include(l => l.Coordinates)
            .ThenInclude(c => c!.Tray);
    }
}