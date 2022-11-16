using System.Text.Json.Serialization;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Sites;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }
    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        [JsonPropertyName("opus")]
        public IEnumerable<ResOPU> OPUs { get; set; } = default!;

        public record ResOPU(int Id, string Name);
    }


    private static Res MapOut(Site s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        OPUs = s.Children.Select(o => new Res.ResOPU(o.Id, o.Name))
    };

    public override void Configure()
    {
        Get(Api.Routes.Sites.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Sites"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var site = await SiteRepo.FirstOrDefaultAsync(new CHNodeWithChildrenSpec<Site, OPU>(req.Id), ct);

        if (site is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(site);
        await SendOkAsync(res, ct);
    }

}