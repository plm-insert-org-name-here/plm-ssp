using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class ReCalibrate : Endpoint<ReCalibrate.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; } = default!;
    public class Req
    {
        public int LocationId { get; set; }
        public reqKoordinates[]? NewTrayCoordinates { get; set; }
        public record reqKoordinates(int X, int Y);
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Detectors.ReCalibrate);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorAndCoordinatesSpec(req.LocationId), ct);

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
        Logger.LogInformation("{@coords}", location.Coordinates);

        var result = await location.SendRecalibrate(DetectorConnection, req.NewTrayCoordinates.Select(c => new CalibrationCoordinates.Koordinates(){X = c.X, Y = c.Y}).ToList());
        var test = result.Unwrap();
        
        Logger.LogInformation("{@test}", test);

        await LocationRepo.SaveChangesAsync();

        await SendNoContentAsync(ct);
    }
}