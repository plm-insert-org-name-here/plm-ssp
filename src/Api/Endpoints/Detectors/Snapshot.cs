using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class Snapshot : Endpoint<Snapshot.Req>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; }= default!;

    public class Req
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.Detectors.Snapshot);
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
            ThrowError("Snapshots can't be requested from detached detectors");
            return;
        }

        var result = await DetectorConnection.RequestSnapshot(detector, req.Type);
        var snapshot = result.Unwrap();

        detector.Location.Snapshot = snapshot;
        await DetectorRepo.SaveChangesAsync(ct);

        await SendBytesAsync(snapshot, cancellation: ct);
    }
}
