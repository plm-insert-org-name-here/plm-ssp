using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Jobs;

public class Rename: Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Jobs.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Jobs"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = await JobRepo.GetByIdAsync(req.Id, ct);

        if (job is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        job.Name = req.Name;

        await JobRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}