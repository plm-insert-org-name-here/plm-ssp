using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class HeartBeatByDetectorIdSpec : Specification<Detector>
{
    public HeartBeatByDetectorIdSpec(int id)
    {
        Query.Where(d => d.Id == id)
            .Include(d => d.HeartBeatLogs)
            .Include(d => d.Location);
    }
}