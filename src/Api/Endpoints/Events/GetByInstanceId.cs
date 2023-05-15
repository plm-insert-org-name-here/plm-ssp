using Api.Endpoints.Locations;
using Domain.Common;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Events;

public class GetByInstanceId : Endpoint<GetByInstanceId.Req, GetByInstanceId.Res>
{
    public IRepository<TaskInstance> InstanceRepo { get; set; } = default!;
    public class Req
    {
        public int InstanceId { get; set; }
    }
    
    public class Res
    {
        public List<EventRes> Events { get; set; }
        public record EventRes(int Id, DateTime Timestamp, bool Success, string? FailureReason, StepRes? Step);
        public record StepRes(int Id, int? OrderNum, TemplateState ExpectedInitialState,
            TemplateState ExpectedSubsequentState, ObjectRes Object);
        
        public record ObjectRes(int Id, string Name, ObjectCoordinates Coordinates);
    }

    private static Res MapOut(List<Event> events)
    {
        var res = new Res
        {
            Events = new List<Res.EventRes>()
        };

        foreach (var _event in events)
        {
            var _object = new Res.ObjectRes(_event.Step.ObjectId, _event.Step.Object.Name,
                _event.Step.Object.Coordinates);
            var _step = new Res.StepRes(_event.StepId, _event.Step.OrderNum, _event.Step.ExpectedInitialState,
                _event.Step.ExpectedSubsequentState, _object);

            var _eventRes = new Res.EventRes
            (
                _event.Id,
                _event.Timestamp,
                _event.Result.Success,
                _event.Result.FailureReason,
                _step
            );

            res.Events.Add(_eventRes);

        }
    
        return res;
    }

    public override void Configure()
    {
        Get(Api.Routes.Events.GetByInstanceId);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var instance = await InstanceRepo.FirstOrDefaultAsync(new TaskInstanceWithEventsByIdSpec(req.InstanceId), ct);
        if (instance is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        var res = MapOut(instance.Events);

        await SendOkAsync(res, ct);
    }
}