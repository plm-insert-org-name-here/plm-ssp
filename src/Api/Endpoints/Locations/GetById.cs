using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Locations;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public bool HasSnapshot { get; set; }

        public OngoingTaskRes? OngoingTask { get; set; }

        public record EventRes(int Id, DateTime Timestamp, EventResult Result, StepRes Step);

        public record StepRes(int Id, int? OrderNum, TemplateState ExInitState,
            TemplateState ExSubsState, ObjectRes Object);

        public record ObjectRes(int Id, string Name, ObjectCoordinates Coords);

        public record OngoingTaskInstanceRes(int Id, TaskInstanceFinalState? FinalState,
            IEnumerable<EventRes> Events, int CurrentOrderNum);

        public record OngoingJobRes(int Id, string Name);

        public record OngoingTaskRes(int Id, string Name, TaskType Type, TaskState State,
            OngoingJobRes Job,
            OngoingTaskInstanceRes? TaskInstance,
            IEnumerable<StepRes> Steps,
            int MaxOrderNum
            );
    }

    private static Res MapOut(Location l)
    {
        var res = new Res
        {
            Id = l.Id,
            Name = l.Name,
            HasSnapshot = l.Snapshot is not null
        };

        // The specification only fetches the ongoing task, if there is one
        var task = l.Tasks.FirstOrDefault();

        if (task is null) return res;

        var steps = task.Steps.Select(s => new Res.StepRes(s.Id, s.OrderNum, s.ExpectedInitialState,
            s.ExpectedSubsequentState,
            new Res.ObjectRes(s.Object.Id, s.Object.Name, s.Object.Coordinates))).ToList();

        var jobRes = new Res.OngoingJobRes(task.Job.Id, task.Job.Name);

        // Only the ongoing task instance is fetched, if there is one
        var taskInstance = task.Instances.FirstOrDefault();

        if (taskInstance is null)
        {
            res.OngoingTask = new Res.OngoingTaskRes(task.Id, task.Name, task.Type, task.State,
                jobRes, null, steps, 0);
            return res;
        }

        var taskInstanceRes = new Res.OngoingTaskInstanceRes(taskInstance.Id,
            taskInstance.FinalState, taskInstance.Events
                .Select(e =>
                    new Res.EventRes(e.Id, e.Timestamp, e.Result,
                        steps.First(s => s.Id == e.StepId))), 0);

        res.OngoingTask = new Res.OngoingTaskRes(task.Id, task.Name, task.Type, task.State, jobRes,
            taskInstanceRes, steps, 0);

        return res;
    }

    public override void Configure()
    {
        Get(Api.Routes.Locations.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location =
            await LocationRepo.FirstOrDefaultAsync(new LocationWithActiveTaskSpec(req.Id), ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(location);
        await SendOkAsync(res, ct);
    }
}