using Domain.Entities.CompanyHierarchy;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Locations;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }
    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public DetectorRes? Detector { get; set; } = default!;


        public record DetectorRes(int Id, string Name);
    }

    private static Res MapOut(Location l) =>
     new()
        {
            Id = l.Id,
            Name = l.Name,
            Detector = l.Detector is null ? null
                : new Res.DetectorRes(l.Detector.Id, l.Detector.Name)
        };

    public override void Configure()
    {
        Get(Api.Routes.Locations.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.Id), ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(location);
        await SendOkAsync(res, ct);
    }

}
