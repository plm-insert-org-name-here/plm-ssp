using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Stations;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<Station> StationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.Stations.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("Stations"));
        Description(x => x
                .Accepts<Req>("application/json")
                .Produces(204)
                .Produces(404),
            clearDefaults: true);
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var station = await StationRepo.GetByIdAsync(req.Id, ct);

        if (station is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // TODO: check detectors below station
        await StationRepo.DeleteAsync(station, ct);

        await SendNoContentAsync(ct);
    }
}
