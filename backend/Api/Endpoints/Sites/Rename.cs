using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;
using Microsoft.AspNetCore.Http;

namespace Api.Endpoints.Sites;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Sites.Update);
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

        site.Name = req.Name;

        await SiteRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }

}