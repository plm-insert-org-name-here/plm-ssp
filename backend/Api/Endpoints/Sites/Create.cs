using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Sites;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;
    public ICHNameUniquenessChecker<Site> NameUniquenessChecker { get; set; } = default!;

    public class Req
    {
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

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
        var result = Site.New(req.Name, NameUniquenessChecker);
        var newSite = result.Unwrap();

        await SiteRepo.AddAsync(newSite, ct);

        var res = MapOut(newSite);
        await SendCreatedAtAsync<Create>(new { newSite.Id }, res, null, null, false, ct);
    }
}
