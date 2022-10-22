using Domain.Entities.CompanyHierarchy;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Tasks;

public class Delete: Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<Task> TaskRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.Tasks.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }
    
    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.GetByIdAsync(req.Id, ct);

        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // TODO: check detectors below 
        await TaskRepo.DeleteAsync(task, ct);

        await SendNoContentAsync(ct);
    }
}