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
        public TaskState State { get; set; }
        public ResInstance? LatestInstance { get; set; }

        public record ResInstance(int Id, TaskInstanceFinalState? FinalState, IEnumerable<ResEvent> Events);

        // TODO(rg): might need to extend with steps and objects
        public record ResEvent(DateTime Timestamp, ResEventResult Result);
        public record ResEventResult(bool Success, string? FailureReason);

    }

    private static Res MapOut(Domain.Entities.TaskAggregate.Task t, TaskInstance? ti)
    {
        var res = new Res
        {
            Id = t.Id,
            Name = t.Name,
            State = t.State
        };

        if (ti is not null)
        {
            var events = ti.Events.Select(e => new Res.ResEvent(e.Timestamp, new Res.ResEventResult(e.Result.Success, e.Result.FailureReason)));
            var resInstance = new Res.ResInstance(ti.Id, ti.FinalState, events);

            res.LatestInstance = resInstance;
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
        var task = await TaskRepo.GetByIdAsync(req.Id, ct);
        var latestInstance = await TaskInstanceRepo.FirstOrDefaultAsync(
            new LatestTaskInstanceByTaskIdWithEventsSpec(req.Id), ct
        );

        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(task, latestInstance);
        await SendOkAsync(res, ct);
    }
}