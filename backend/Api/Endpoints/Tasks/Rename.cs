using Domain.Entities.TaskAggregate;
using FastEndpoints;
using Infrastructure.Database;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class Rename: Endpoint<Rename.Req, EmptyResponse>
{
    public IRepository<Task> TaskRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public override void Configure()
    {
        Put(Api.Routes.Tasks.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.GetByIdAsync(req.Id, ct);

        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        task.Name = req.Name;

        await TaskRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}