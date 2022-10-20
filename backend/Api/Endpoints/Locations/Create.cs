using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Locations;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Station> StationRepo { get; set; } = default!;
    public class Req
    {
        public int ParentStationId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Res MapOut(Location l) => new()
    {
        Id = l.Id,
        Name = l.Name
    };

    public override void Configure()
    {
        Post(Api.Routes.Locations.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var station = await StationRepo.FirstOrDefaultAsync(new StationWithLocationsSpec(req.ParentStationId), ct);

        if (station is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var location = new Location
        {
            Name = req.Name
        };

        station.Children.Add(location);

        await StationRepo.SaveChangesAsync(ct);

        var res = MapOut(location);

        await SendCreatedAtAsync<Create>(new { location.Id }, res, null, null, false, ct);
    }
}
