using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = System.Threading.Tasks.Task;

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

        public DetectorRes? Detector { get; set; }

        public OngoingTaskRes? OngoingTask { get; set; }

        public record DetectorRes(int Id, string Name, DetectorState State);

        public record EventRes(int Id, DateTime Timestamp, bool Success, string? FailureReason, StepRes? Step);

        public record StepRes(int Id, int? OrderNum, TemplateState ExInitState,
            TemplateState ExSubsState, ObjectRes Object);

        public record ObjectRes(int Id, string Name, ObjectCoordinates Coords);

        public record OngoingTaskInstanceRes(int Id, TaskInstanceState State,
            IEnumerable<EventRes> Events, int CurrentOrderNum, IEnumerable<StepRes> CurrentOrderNumRemainingSteps);

        public record OngoingJobRes(int Id, string Name);

        public record OngoingTaskRes(int Id, string Name, TaskType Type,
            OngoingJobRes Job,
            OngoingTaskInstanceRes? OngoingInstance,
            IEnumerable<StepRes> Steps,
            int MaxOrderNum
        );
    }

    private static IEnumerable<Res.StepRes> MapSteps(IEnumerable<Step> steps)
    {
        return steps.Select(s => new Res.StepRes(s.Id, s.OrderNum, s.ExpectedInitialState,
            s.ExpectedSubsequentState,
            new Res.ObjectRes(s.Object.Id, s.Object.Name, s.Object.Coordinates)));
    }

    private static Res MapOut(Location l)
    {
        var res = new Res
        {
            Id = l.Id,
            Name = l.Name,
            HasSnapshot = l.Snapshot is not null,
        };

        if (l.Detector is not null)
        {
            res.Detector = new Res.DetectorRes(l.Detector.Id, l.Detector.Name, l.Detector.State);
        }

        if (l.OngoingTask is null) return res;

        var steps = MapSteps(l.OngoingTask.Steps);

        var jobRes = new Res.OngoingJobRes(l.OngoingTask.Job.Id, l.OngoingTask.Job.Name);

        // Only the ongoing task instance is fetched, if there is one
        var taskInstance = l.OngoingTask.OngoingInstance;

        if (taskInstance is null)
        {
            res.OngoingTask = new Res.OngoingTaskRes(l.OngoingTask.Id, l.OngoingTask.Name, l.OngoingTask.Type,
                jobRes, null, steps, l.OngoingTask.MaxOrderNum);
            return res;
        }

        var currentOrderNumRemainingSteps = MapSteps(l.OngoingTask.Steps.Where(s =>
            s.OrderNum == taskInstance.CurrentOrderNum && taskInstance.RemainingStepIds.Contains(s.Id)));

        var taskInstanceRes = new Res.OngoingTaskInstanceRes(taskInstance.Id,
            taskInstance.State, taskInstance.Events
                .Select(e =>
                    new Res.EventRes(e.Id, e.Timestamp, e.Result.Success, e.Result.FailureReason,
                        steps.First(s => s.Id == e.StepId))), taskInstance.CurrentOrderNum,
            currentOrderNumRemainingSteps);

        res.OngoingTask = new Res.OngoingTaskRes(l.OngoingTask.Id, l.OngoingTask.Name, l.OngoingTask.Type, jobRes,
            taskInstanceRes, steps, l.OngoingTask.MaxOrderNum);

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