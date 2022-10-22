using Domain.Entities;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Jobs;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.Jobs.Delete);
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
        
        await JobRepo.DeleteAsync(job, ct);

        await SendNoContentAsync(ct);
    }
}