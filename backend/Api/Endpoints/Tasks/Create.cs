using Domain.Entities;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure.Database;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class Create: Endpoint<Create.Req, Create.Res>
{
    public IRepository<Job> JobRepo { get; set; } = default!;
    public class Req
    {
        public int ParentJobId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Res MapOut(Task task) => new()
    {
        Id = task.Id,
        Name = task.Name
    };
    
    public override void Configure()
    {
        Post(Api.Routes.Tasks.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }
    
    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = await JobRepo.FirstOrDefaultAsync(new JobWithTasksSpec(req.ParentJobId), ct);

        if (job is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var task = new Task
        {
            Name = req.Name
        };

        job.Tasks.Add(task);

        await JobRepo.SaveChangesAsync(ct);

        var res = MapOut(task);

        await SendCreatedAtAsync<Create>(new { task.Id }, res, null, null, false, ct);
    }
}