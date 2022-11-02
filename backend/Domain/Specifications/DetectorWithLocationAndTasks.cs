using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public class DetectorWithLocationAndTasks : Specification<Detector>, ISingleResultSpecification
{
    public DetectorWithLocationAndTasks(int id)
    {
        Query.Where(d => d.Id == id).Include(d => d.Location).ThenInclude(l => l!.Tasks);
    }
}