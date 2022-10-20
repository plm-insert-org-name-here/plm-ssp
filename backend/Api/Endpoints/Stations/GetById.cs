using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Stations;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Station> StationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }
    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public IEnumerable<LocationRes> Locations { get; set; } = default!;

        public record LocationRes(int Id, string Name);
    }

    private static Res MapOut(Station s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Locations = s.Children.Select(o => new Res.LocationRes(o.Id, o.Name))
    };

    public override void Configure()
    {
        Get(Api.Routes.Stations.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Stations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var station = await StationRepo.FirstOrDefaultAsync(new StationWithLocationsSpec(req.Id), ct);

        if (station is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(station);
        await SendOkAsync(res, ct);
    }

}