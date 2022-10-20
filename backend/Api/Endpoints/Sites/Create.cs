using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Sites;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;

    public class Req
    {
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Site MapIn(Req r) =>
        new()
        {
            Name = r.Name
        };

    private static Res MapOut(Site s) =>
        new()
        {
            Id = s.Id,
            Name = s.Name
        };

    public override void Configure()
    {
        Post(Api.Routes.Sites.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Sites"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var site = MapIn(req);

        await SiteRepo.AddAsync(site, ct);

        var res = MapOut(site);
        await SendCreatedAtAsync<Create>(new { site.Id }, res, null, null, false, ct);
    }
}
