using Domain.Common;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class EventPost: Endpoint<EventPost.Req, EmptyResponse>
{
    public IRepository<Task> TaskRepo { get; set; } = default!;
    public class Req
    {
        public int TaskId { get; set; }
        public int StepId { get; set; }
        public bool EventResult { get; set; }
        public string FailureReason { get; set; } = default!;
    }

    public override void Configure()
    {
        Post(Api.Routes.Tasks.EventPost);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(req.TaskId),ct);
        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var instance = task.Instances.FirstOrDefault(i => i.TaskId == task.Id);
        if (instance is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var eventResult = new EventResult()
        {
            Success = req.EventResult,
            FailureReason = req.FailureReason
        };
        
        Event newEvent = Event.Create(DateTime.Now, eventResult, req.StepId, instance.Id);
        
        instance.Events.Add(newEvent);
        await TaskRepo.SaveChangesAsync(ct);
        
        await SendOkAsync(ct);
        return;
    }
}
