using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Tasks;

public class GetById: Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Domain.Entities.TaskAggregate.Task> TaskRepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public TaskStatus Status { get; set; } = default!;
        public bool? Ordered { get; set; }

    }
    
    private static Res MapOut(Domain.Entities.TaskAggregate.Task t) => new()
    {
        Id = t.Id,
        Name = t.Name
    };

    public override void Configure()
    {
        Get(Api.Routes.Tasks.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }
    
    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(req.Id), ct);

        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(task);
        await SendOkAsync(res, ct);
    }
}