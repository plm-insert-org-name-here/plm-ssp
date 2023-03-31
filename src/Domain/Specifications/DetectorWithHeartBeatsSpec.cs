using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class DetectorWithHeartBeatsSpec : Specification<Detector>
{
    public DetectorWithHeartBeatsSpec(int id)
    {
        Query.Where(d => d.Id == id).Include(d => d.HeartBeatLogs);
    }
    
}