using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Sites;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.Sites.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("Sites"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var site = await SiteRepo.GetByIdAsync(req.Id, ct);

        if (site is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // TODO: check detectors below site
        await SiteRepo.DeleteAsync(site, ct);

        await SendNoContentAsync(ct);
    }
}