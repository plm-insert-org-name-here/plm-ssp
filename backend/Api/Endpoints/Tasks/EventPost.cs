using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class EventPost: Endpoint<EventPost.Req, EmptyResponse>
{
    public IRepository<Task> TaskRepo { get; set; } = default!;
    public IRepository<Location> LocationRepo { get; set; } = default!;
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

        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(task.LocationId), ct);
        if (!Equals(HttpContext.Connection.RemoteIpAddress, location.Detector.IpAddress))
        {
            //"remote ip address is not equal with the detector's ip!"
            await SendStringAsync(HttpContext.Connection.RemoteIpAddress.ToString());
            return;
        }

        var instance = task.Instances.FirstOrDefault(i => i.TaskId == task.Id);
        if (instance is null || instance.FinalState != null)
        {
            await SendStringAsync(instance.Id.ToString());
            return;
        }

        var eventResult = new EventResult()
        {
            Success = req.EventResult,
            FailureReason = req.FailureReason
        };
        
        Event newEvent = Event.Create(DateTime.Now, eventResult, req.StepId, instance.Id);

        if (req.EventResult)
        {
            if (!instance.Remaining.Contains(req.StepId))
            {
                ThrowError("invalid stepid");
                return;
            }
            if (instance.IsEnded(req.StepId))
            {
                instance.FinalState = TaskInstanceFinalState.Completed;
            }
        }
        
        instance.Events.Add(newEvent);
        await TaskRepo.SaveChangesAsync(ct);
        
        await SendOkAsync(ct);
        return;
    }
}
