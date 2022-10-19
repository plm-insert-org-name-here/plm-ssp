using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;

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
