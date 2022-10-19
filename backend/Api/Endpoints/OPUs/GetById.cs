using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.OPUs;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<OPU> OPURepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        // public List<Line> Lines { get; set; } = default!;
        // public Site Site { get; set; } = default!;
        // public int SiteId { get; set; }
    }

    private static Res MapOut(OPU o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        // Lines = o.Lines,
        // Site = o.Site,
        // SiteId = o.SiteId
    };

    public override void Configure()
    {
        Get(Api.Routes.OPUs.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("OPUs"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var opu = await OPURepo.GetByIdAsync(req.Id, ct);

        if (opu is null)
        {
            await SendNotFoundAsync();
            return;
        }

        var res = MapOut(opu);
        await SendOkAsync(res, ct);
    }
}