using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public class DetectorWithLocationAndTasksSpec : Specification<Detector>, ISingleResultSpecification
{
    public DetectorWithLocationAndTasksSpec(int id)
    {
        Query.Where(d => d.Id == id)
            .Include(d => d.Location)
            .ThenInclude(l => l!.Tasks);
    }
}