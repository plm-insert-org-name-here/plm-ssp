using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Sites;

public class List : EndpointWithoutRequest<IEnumerable<List.Res>>
{
    public IRepository<Site> SiteRepo { get; set; } = default!;


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
        Get(Api.Routes.Sites.List);
        AllowAnonymous();
        Options(x => x.WithTags("Sites"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sites = await SiteRepo.ListAsync(ct);

        await SendOkAsync(sites.Select(MapOut), ct);
    }
}