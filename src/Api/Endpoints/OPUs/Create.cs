using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.OPUs;
public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;
    public ICHNameUniquenessChecker<Site, OPU> NameUniquenessChecker
    {
        get;
        set;
    } = default!;

    public class Req
    {
        public int ParentSiteId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Res MapOut(OPU opu) => new()
    {
        Id = opu.Id,
        Name = opu.Name
    };

    public override void Configure()
    {
        Post(Api.Routes.OPUs.Create);
        AllowAnonymous();
        Options(x => x.WithTags("OPUs"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var site = await SiteRepo.FirstOrDefaultAsync(new CHNodeWithChildrenSpec<Site, OPU>(req.ParentSiteId), ct);

        if (site is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var result = site.AddChildNode(req.Name, NameUniquenessChecker);
        var newOpu = result.Unwrap();

        await SiteRepo.SaveChangesAsync(ct);

        var res = MapOut(newOpu);

        await SendCreatedAtAsync<Create>(new { newOpu.Id }, res, null, null, false, ct);
    }
}