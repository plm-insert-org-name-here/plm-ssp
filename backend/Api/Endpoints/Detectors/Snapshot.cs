using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class Snapshot : Endpoint<Snapshot.Req>
{
    public readonly IRepository<Detector> DetectorRepo = default!;
    public readonly IDetectorConnection DetectorConnection = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.Detectors.Snapshot);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }
    
    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorWithLocationAndTasks(req.Id), ct);

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
            ThrowError("Snapshots can't be requested from detached detectors");
            return;
        }

        var snapshot = await DetectorConnection.RequestSnapshot(detector);

        await SendBytesAsync(snapshot, cancellation: ct);
    }
}