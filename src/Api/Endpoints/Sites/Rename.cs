using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Sites;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;
    public ICHNameUniquenessChecker<Site> NameUniquenessChecker { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Sites.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Sites"));
        Description(x => x
                .Accepts<Req>("application/json")
                .Produces(204)
                .ProducesProblemFE()
                .Produces(404),
            clearDefaults: true);
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var site = await SiteRepo.GetByIdAsync(req.Id, ct);

        if (site is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        site.Rename(req.Name, NameUniquenessChecker).Unwrap();

        await SiteRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}