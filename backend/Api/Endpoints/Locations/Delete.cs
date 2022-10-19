using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Locations;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.Locations.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.GetByIdAsync(req.Id, ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // TODO: check detector on location; only delete location if detector is inactive,
        // i.e. not monitoring
        await LocationRepo.DeleteAsync(location, ct);

        await SendNoContentAsync(ct);
    }
}
