using Ardalis.Specification;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Specifications;

public sealed class OPUWithLinesSpec : Specification<OPU>
{
    public OPUWithLinesSpec(int id)
    {
        Query.Where(o => o.Id == id).Include(o => o.Children);
    }
}