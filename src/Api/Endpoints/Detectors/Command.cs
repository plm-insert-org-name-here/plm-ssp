using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class Command : Endpoint<Command.Req, EmptyResponse>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public DetectorCommandService CommandService { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public DetectorCommand Command { get; set; } = default!;
    }

    public override void Configure()
    {
        Post(Api.Routes.Detectors.Command);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.GetByIdAsync(req.Id, ct);

        if (detector is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var result = await CommandService.HandleCommand(detector, req.Command, ct);
        result.Unwrap();

        await DetectorRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}