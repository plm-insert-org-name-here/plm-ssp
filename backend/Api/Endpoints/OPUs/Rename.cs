using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.OPUs;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<OPU> OpuRepo { get; set; } = default!;

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
        var opu = await OpuRepo.GetByIdAsync(req.Id, ct);

        if (opu is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        opu.Name = req.Name;

        await OpuRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}