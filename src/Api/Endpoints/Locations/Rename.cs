using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Locations;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public ICHNameUniquenessChecker<Station, Location> NameUniquenessChecker
    {
        get;
        set;
    } = default!;

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
        Description(x => x
                .Accepts<Req>("application/json")
                .Produces(204)
                .ProducesProblemFE()
                .Produces(404),
            clearDefaults: true);
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new CHNodeWithParentSpec<Station, Location>(req.Id), ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        location.Rename(req.Name, NameUniquenessChecker).Unwrap();

        await LocationRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}
