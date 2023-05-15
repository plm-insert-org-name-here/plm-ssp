using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure.OpenApi;
using NJsonSchema;
using NSwag.Annotations;

namespace Api.Endpoints.Locations;

public class GetSnapshot : Endpoint<GetSnapshot.Req>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.Locations.GetSnapshot);
        AllowAnonymous();
        Options(x =>
        {
            x.WithTags("Locations");
       });
        Description(x =>
        {
            x.Produces(200, null, "application/octet-stream");
        });
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location =
            await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.Id), ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (location.Snapshot is null)
        {
            ThrowError("Location does not have a Snapshot");
            return;
        }

        await SendBytesAsync(location.Snapshot, "snapshot.jpeg", "application/octet-stream", null, false, ct);
    }
}