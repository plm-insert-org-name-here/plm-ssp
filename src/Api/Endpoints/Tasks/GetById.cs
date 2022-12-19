using Domain.Common;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = System.Threading.Tasks.Task;

namespace Api.Endpoints.Tasks;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Domain.Entities.TaskAggregate.Task> TaskRepo { get; set; } = default!;
    public IRepository<TaskInstance> TaskInstanceRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int MaxOrderNum { get; set; }
        public TaskType TaskType { get; set; }
        public ResLocation Location { get; set; } = default!;
        public ResJob Job { get; set; } = default!;
        public ResInstance? OngoingInstance { get; set; }

        public record ResLocation(int Id, string Name);
        public record ResJob(int Id, string Name);

        public record ResInstance(int Id, TaskInstanceState State, IEnumerable<ResEvent> Events);

        // TODO(rg): might need to extend with steps and objects
        public record ResEvent(DateTime Timestamp, bool Success, string? FailureReason);

    }

    private static Res MapOut(Domain.Entities.TaskAggregate.Task t)
    {
        var res = new Res
        {
            Id = t.Id,
            Name = t.Name,
            MaxOrderNum = t.MaxOrderNum,
            TaskType = t.Type,
            Location = new Res.ResLocation(t.Location.Id, t.Location.Name),
            Job = new Res.ResJob(t.Job.Id, t.Job.Name)
        };

        if (t.OngoingInstance is not null)
        {
            var events = t.OngoingInstance.Events.Select(e => new Res.ResEvent(e.Timestamp, e.Result.Success, e.Result.FailureReason));
            res.OngoingInstance = new Res.ResInstance(t.OngoingInstance.Id, t.OngoingInstance.State, events);
        }

        return res;
    }

    public override void Configure()
    {
        Get(Api.Routes.Tasks.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.FirstOrDefaultAsync(
            new TaskWithOngoingInstanceSpec(req.Id), ct);

        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(task);
        await SendOkAsync(res, ct);
    }
}