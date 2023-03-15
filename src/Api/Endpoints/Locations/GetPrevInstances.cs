using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Locations;
using Task = Domain.Entities.TaskAggregate.Task;

public class GetPrevInstances : Endpoint<GetPrevInstances.Req, GetPrevInstances.Res>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public class Req
    {
        public int LocationId { get; set; }
    }
    
    public class Res
    {
        public List<ResTaskInstance> Instances { get; set; } = default!;

        public record ResTaskInstance(int Id, TaskInstanceState? FinalState, IEnumerable<ResEvent> Events, int TaskId);

        public record ResEvent(int Id, DateTime TimeStamp, bool EventResultSuccess, string? FailureReason, int StepId, int TaskInstanceId);

    }
    
    public override void Configure()
    {
        Get(Api.Routes.Locations.GetPrevInstances);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithTaskInstancesSpec(req.LocationId), ct);
        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = new Res
        {
            Instances = new List<Res.ResTaskInstance>()
        };
        foreach (var task in location.Tasks)
        {
            foreach (var instance in task.Instances)
            {
                var resEvents = instance.Events.Select(e =>
                    new Res.ResEvent(e.Id, e.Timestamp, e.Result.Success,  e.Result.FailureReason, e.StepId, e.TaskInstanceId));

                var resInstance = new Res.ResTaskInstance(instance.Id, instance.State, resEvents, instance.TaskId);
                res.Instances.Add(resInstance);
            }
        }
        

        await SendOkAsync(res, ct);
    }

}