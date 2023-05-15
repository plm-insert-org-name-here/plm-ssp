using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class Stream : Endpoint<Stream.Req>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; }= default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.Detectors.Stream);
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

        var result = await DetectorConnection.RequestStream(detector);
        var stream = result.Unwrap();

        // NOTE(rg): content type is necessary if we want to view the stream directly from the browser; otherwise,
        // it will be downloaded as a file
        await SendStreamAsync(stream, null, cancellation: ct, contentType: "multipart/x-mixed-replace; boundary=frame");
    }
}