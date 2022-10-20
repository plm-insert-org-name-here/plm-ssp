using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Lines;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Line> LineRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Lines.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Lines"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var line = await LineRepo.GetByIdAsync(req.Id, ct);

        if (line is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        line.Name = req.Name;

        await LineRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}