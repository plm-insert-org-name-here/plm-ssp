using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Events;

public class Create: Endpoint<Create.Req, EmptyResponse>
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
        Post(Api.Routes.Events.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Events"));
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
        if (location is null)
        {
            ThrowError("Location does not exist");
            return;
        }

        if (location.Detector is null)
        {
            ThrowError("Location does not have a Detector attached to it");
            return;
        }

        if (!Equals(HttpContext.Connection.RemoteIpAddress, location.Detector.IpAddress))
        {
            ThrowError("Remote IP address is not equal with the detector's IP");
            return;
        }

        var instance = task.Instances.FirstOrDefault(i => i.TaskId == task.Id);
        if (instance is null || instance.FinalState != null)
        {
            ThrowError("Task instance does not exist, or it's finished");
            return;
        }

        var eventResult = new EventResult
        {
            Success = req.EventResult,
            FailureReason = req.FailureReason
        };

        var newEvent = Event.Create(DateTime.Now, eventResult, req.StepId, instance.Id);

        if (req.EventResult)
        {
            if (!instance.Remaining.Contains(req.StepId))
            {
                ThrowError("invalid step Id");
                return;
            }

            if (instance.IsEnded(req.StepId))
            {
                instance.FinalState = TaskInstanceFinalState.Completed;
            }
        }

        instance.Events.Add(newEvent);
        await TaskRepo.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}
