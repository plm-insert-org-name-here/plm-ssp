using Domain.Common;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class GetCurrentInstance : Endpoint<GetCurrentInstance.Req, GetCurrentInstance.Res>
{
    public IRepository<Task> TaskRepo { get; set; } = default!;

    public class Req
    {
        public int TaskId { get; set; }
    }

    public class Res
    {
        public ResTaskInstance Instance { get; set; } = default!;

        public record ResTaskInstance(int Id, TaskInstanceState? FinalState, IEnumerable<ResEvent> Events, int TaskId);

        public record ResEvent(int Id, DateTime TimeStamp, bool EventResultSuccess, string? FailureReason, int StepId, int TaskInstanceId);

    }

    public override void Configure()
    {
        Get(Api.Routes.Tasks.GetInstance);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithOngoingInstanceSpec(req.TaskId), ct);

        if (task?.OngoingInstance is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var resEvents = task.OngoingInstance.Events.Select(e =>
            new Res.ResEvent(e.Id, e.Timestamp, e.Result.Success,  e.Result.FailureReason, e.StepId, e.TaskInstanceId));

        var res = new Res
        {
            Instance = new Res.ResTaskInstance(task.OngoingInstance.Id, task.OngoingInstance.State, resEvents, task.OngoingInstance.TaskId)
        };

        await SendOkAsync(res, ct);
    }
}