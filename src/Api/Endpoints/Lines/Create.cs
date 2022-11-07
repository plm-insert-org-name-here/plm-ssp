using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Lines;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<OPU> OpuRepo { get; set; } = default!;

    public ICHNameUniquenessChecker<OPU, Line> NameUniquenessChecker
    {
        get;
        set;
    } = default!;

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
        var opu = await OpuRepo.FirstOrDefaultAsync(new CHNodeWithChildrenSpec<OPU, Line>(req.OPUId), ct);

        if (opu is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var result = opu.AddChildNode(req.Name, NameUniquenessChecker);
        var newLine = result.Unwrap();

        await OpuRepo.SaveChangesAsync(ct);

        var res = MapOut(newLine);
        await SendCreatedAtAsync<Create>(new { newLine.Id }, res, null, null, false, ct);
    }
}