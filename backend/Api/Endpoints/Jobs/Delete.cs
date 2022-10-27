using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

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
        
        //TODO: check if it has any active detectors under it
        if (job is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        foreach (var task in job.Tasks)
        {
            //TODO: delete task but, every child of it
        }
        
        await JobRepo.DeleteAsync(job, ct);

        await SendNoContentAsync(ct);
    }
}