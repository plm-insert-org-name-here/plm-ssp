using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Stations;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Line> LineRepo { get; set; } = default!;
    public class Req
    {
        public int ParentLineId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Res MapOut(Station station) => new()
    {
        Id = station.Id,
        Name = station.Name
    };

    public override void Configure()
    {
        Post(Api.Routes.Stations.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Stations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var line = await LineRepo.FirstOrDefaultAsync(new LineWithStationsSpec(req.ParentLineId), ct);

        if (line is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var station = new Station
        {
            Name = req.Name
        };

        line.Children.Add(station);

        await LineRepo.SaveChangesAsync(ct);

        var res = MapOut(station);

        await SendCreatedAtAsync<Create>(new { station.Id }, res, null, null, false, ct);
    }
}
