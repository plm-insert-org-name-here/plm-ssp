using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class Detach: Endpoint<Detach.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public class Req
    {
        public int LocationId { get; set; }
    }
    
    public override void Configure()
    {
        Post(Api.Routes.Detectors.Detach);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.LocationId), ct);
        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        var result = location.DetachDetector();
        await LocationRepo.SaveChangesAsync(ct);
        
        await SendStringAsync(result.ToString());
        return;
    }
}