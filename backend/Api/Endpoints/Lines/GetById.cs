using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Lines;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Line> LineRepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public IEnumerable<ResStation> Stations { get; set; } = default!;
        public record ResStation(int Id, string Name);
    }

    public static Res MapOut(Line l) => new()
    {
        Id = l.Id,
        Name = l.Name,
        Stations = l.Children.Select(s => new Res.ResStation(s.Id, s.Name))
    };

    public override void Configure()
    {
        Get(Api.Routes.Lines.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Lines"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var line = await LineRepo.FirstOrDefaultAsync(new LineWithStationsSpec(req.Id), ct);

        if (line is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(line);
        await SendOkAsync(res, ct);
    }
}