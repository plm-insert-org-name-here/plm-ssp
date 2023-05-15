using Ardalis.Specification;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Specifications;

public sealed class DetectorsWithLocationSpec : Specification<Detector>
{
    public DetectorsWithLocationSpec()
    {
        Query.Include(d => d.Location);
    }
}