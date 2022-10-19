using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Lines;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Line> LineRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; } = default!;
        public int OPUId { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Post(Api.Routes.Lines.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Lines"));
    }

    private static Line MapIn(Req r) => 
        new()
        {
            Name = r.Name,
            OPUId = r.OPUId
        };

    private static Res MapOut(Line l) =>
        new()
        {
            Id = l.Id,
            Name = l.Name
        };

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var line = MapIn(req);

        await LineRepo.AddAsync(line, ct);

        var res = MapOut(line);
        await SendCreatedAtAsync<Create>(new { line.Id }, res, null, null, false, ct);
    }
}