using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.OPUs;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<OPU> OpuRepo { get; set; } = default!;

    public ICHNameUniquenessChecker<ICHNodeWithChildren<OPU>, OPU> NameUniquenessChecker
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
        Put(Api.Routes.OPUs.Update);
        AllowAnonymous();
        Options(x => x.WithTags("OPUs"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var opu = await OpuRepo.FirstOrDefaultAsync(new CHNodeWithParentSpec<Site, OPU>(req.Id), ct);

        if (opu is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        opu.Rename(req.Name, NameUniquenessChecker).Unwrap();

        await OpuRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}