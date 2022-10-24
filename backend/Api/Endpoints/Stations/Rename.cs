using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Stations;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Station> StationRepo { get; set; } = default!;

    public ICHNameUniquenessChecker<Line, Station> NameUniquenessChecker
    {
        get;
        set;
    } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Stations.Update);
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

        station.Rename(req.Name, NameUniquenessChecker).Unwrap();

        await StationRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}