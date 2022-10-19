using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Locations;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Locations.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.GetByIdAsync(req.Id, ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        location.Name = req.Name;

        await LocationRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}
