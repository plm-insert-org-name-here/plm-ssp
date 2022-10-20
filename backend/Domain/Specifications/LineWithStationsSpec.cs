using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class LineWithStationsSpec : Specification<Line>
{
    public LineWithStationsSpec(int id)
    {
        Query.Where(l => l.Id == id).Include(l => l.Children);
    }
}