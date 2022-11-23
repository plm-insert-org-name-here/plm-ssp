using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class DetectorByIdWithLocationSpec : Specification<Detector>
{
    public DetectorByIdWithLocationSpec(int id)
    {
        Query.Where(d => d.Id == id).Include(d => d.Location);
    }
}