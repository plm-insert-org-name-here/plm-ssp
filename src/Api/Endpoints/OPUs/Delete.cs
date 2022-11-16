using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.OPUs;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<OPU> OpuRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.OPUs.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("OPUs"));
        Description(x => x
                .Accepts<Req>("application/json")
                .Produces(204)
                .Produces(404),
            clearDefaults: true);
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var opu = await OpuRepo.GetByIdAsync(req.Id, ct);

        if (opu is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // TODO: check detectors below opu
        await OpuRepo.DeleteAsync(opu, ct);

        await SendNoContentAsync(ct);
    }
}