using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class Attach: Endpoint<Attach.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public class Req 
    {
        public int LocationId { get; set; }
        public int DetectorId { get; set; }
    }
    
    public override void Configure()
    {
        Post(Api.Routes.Detectors.Attach);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.LocationId), ct);
        var detector = await DetectorRepo.GetByIdAsync(req.DetectorId, ct);

        if (detector is null || location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        var result = location.AttachDetector(detector);
        await LocationRepo.SaveChangesAsync(ct);
        
        await SendStringAsync(result.ToString());
        return;
    }
}