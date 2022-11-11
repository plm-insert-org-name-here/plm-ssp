using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class ReCalibrate : Endpoint<ReCalibrate.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public class Req
    {
        public int LocationId { get; set; }
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Detectors.ReCalibrate);
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

        if (location.Detector is null)
        {
            ThrowError("This location has no active detector!");
            return;
        }
        
        var result = location.Detector.SendRecalibrate(location.OriginalCoordinates);
        result.Unwrap();
        
        await SendNoContentAsync(ct);
    }
}