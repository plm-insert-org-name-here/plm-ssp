using Application.Services;
using FastEndpoints;

namespace Api.Endpoints;

public class StartCheckDetectorState : EndpointWithoutRequest
{
    public IDetectorStatusService DetectorStatusService { get; set; } = default!;
    public override void Configure()
    {
        Get(Api.Routes.Other.StartCheckDetectorState);
        AllowAnonymous();
        Options(x => x.WithTags("Other"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        DetectorStatusService.CheckStatus();
    }
}