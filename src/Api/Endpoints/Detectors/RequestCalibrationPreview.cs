using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class RequestCalibrationPreview : Endpoint<RequestCalibrationPreview.Req>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; }= default!;
    
    public class Req
    {
        public int Id { get; set; }
        public int[] NewTrayPoints { get; set; } = default!;
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Detectors.RequestCalibrationPreview);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorWithLocationAndTasksSpec(req.Id), ct);

        if (detector is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        if (detector.State is DetectorState.Off)
        {
            ThrowError("Detector is offline");
            return;
        }
        
        if (detector.Location is null)
        {
            ThrowError("Preview can't be requested from detached detectors");
            return;
        }
        
        var old = detector.Location.Coordinates;

        if (old is null)
        {
            ThrowError("old is null");
            return; 
        }
        
        var result = await DetectorConnection.RequestCalibrationPreview(detector, old, req.NewTrayPoints);
        var preview = result.Unwrap();

        await SendBytesAsync(preview, cancellation: ct);
    }
}