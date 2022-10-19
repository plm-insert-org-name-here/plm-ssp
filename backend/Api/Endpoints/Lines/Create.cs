using Domain.Entities.CompanyHierarchy;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Lines;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<OPU> OpuRepo { get; set; } = default!;

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

    private static Res MapOut(Line l) =>
        new()
        {
            Id = l.Id,
            Name = l.Name
        };

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var opu = await OpuRepo.FirstOrDefaultAsync(new OPUWithLinesSpec(req.OPUId), ct);

        if (opu is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var line = new Line
        {
            Name = req.Name
        };

        opu.Lines.Add(line);
        await OpuRepo.SaveChangesAsync(ct);

        var res = MapOut(line);
        await SendCreatedAtAsync<Create>(new { line.Id }, res, null, null, false, ct);
    }
}