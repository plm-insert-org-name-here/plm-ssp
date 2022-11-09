using Domain.Common;
using Domain.Entities;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;
using Object = Domain.Entities.TaskAggregate.Object;

namespace Api.Endpoints.Tasks;

public class Update : Endpoint<Update.Req, EmptyResponse>
{
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string? NewName { get; set; } = default!;
        public TaskType? NewType { get; set; }
        public int ParentJobId { get; set; }

        public IEnumerable<NewObjectReq>? NewObjects { get; set; }
        public IEnumerable<ModObjectReq>? ModifiedObjects { get; set; }
        public IEnumerable<int>? DeletedObjects { get; set; }
        public IEnumerable<NewStepReq>? NewSteps { get; set; }
        public IEnumerable<ModStepReq>? ModifiedSteps { get; set; }
        public IEnumerable<int>? DeletedSteps { get; set; }

        // ReSharper disable once ClassNeverInstantiated.Global
        public record NewObjectReq(string Name, ObjectCoordinates Coordinates);

        // ReSharper disable once ClassNeverInstantiated.Global
        public record ModObjectReq(int Id, string Name, ObjectCoordinates Coordinates);

        // ReSharper disable once ClassNeverInstantiated.Global
        public record NewStepReq(int OrderNum, TemplateState ExpectedInitialState,
            TemplateState ExpectedSubsequentState, string ObjectName);

        // ReSharper disable once ClassNeverInstantiated.Global
        public record ModStepReq(int Id, int OrderNum, TemplateState ExpectedInitialState,
            TemplateState ExpectedSubsequentState, string ObjectName);
    }

    public override void Configure()
    {
        Put(Api.Routes.Tasks.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
        Summary(x => x.ExampleRequest = new Req
        {
            Id = 1,
            ParentJobId = 1,
            DeletedObjects = new[] { 2 },
            DeletedSteps = new[] { 1 },
            ModifiedObjects = new[]
            {
                new Req.ModObjectReq(1, "Object One", new ObjectCoordinates { X = 10, Y = 10, Width = 10, Height = 10 })
            },
            ModifiedSteps = new[]
                { new Req.ModStepReq(2, 2, TemplateState.Present, TemplateState.Missing, "Object One") },
            NewObjects = new[]
            {
                new Req.NewObjectReq("Object 4",
                    new ObjectCoordinates() { X = 100, Y = 100, Width = 100, Height = 100 })
            },
            NewSteps = new[] { new Req.NewStepReq(4, TemplateState.Present, TemplateState.Missing, "Object 4") }
        });
    }

    // NOTE(rg): it's safe to update Tasks while they're being ran. Detectors only ask for the Task once, when starting it.
    // Because of this, we can't mess up the execution of a Task if we update in during the execution.
    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = await JobRepo.FirstOrDefaultAsync(new JobWithSpecificTaskSpec(req.ParentJobId, req.Id), ct);

        if (job is null)
        {
            ThrowError("Parent Job does not exist");
            return;
        }

        var task = job.Tasks.FirstOrDefault(t => t.Id == req.Id);
        if (task is null || task.JobId != job.Id)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (req.NewName is not null && req.NewName != task.Name)
            task.Rename(req.NewName, job).Unwrap();

        if (req.DeletedObjects is not null) task.RemoveObjects(req.DeletedObjects);
        if (req.DeletedSteps is not null) task.RemoveSteps(req.DeletedSteps);

        if (req.ModifiedObjects is not null)
            foreach (var (id, name, coords) in req.ModifiedObjects)
            {
                task.ModifyObject(id, name, coords).Unwrap();
            }

        if (req.NewObjects is not null)
            task.AddObjects(req.NewObjects.Select(o => Object.Create(o.Name, o.Coordinates, task).Unwrap()));

        if (req.ModifiedSteps is not null)
            foreach (var (id, orderNum, exInitState, exSubsState, objectName) in req.ModifiedSteps)
            {
                var referencedObject = task.Objects.FirstOrDefault(o => o.Name == objectName);
                if (referencedObject is null)
                {
                    ThrowError("Referenced Object does not exist within the Task");
                    return;
                }

                task.ModifyStep(id, orderNum, exInitState, exSubsState, referencedObject).Unwrap();
            }

        if (req.NewSteps is not null)
            task.AddSteps(req.NewSteps.Select(s =>
                {
                    var referencedObject = task.Objects.FirstOrDefault(o => o.Name == s.ObjectName);
                    if (referencedObject is null)
                    {
                        ThrowError("Referenced Object does not exist within the Task");

                    }

                    return Step.Create(
                        s.OrderNum,
                        s.ExpectedInitialState,
                        s.ExpectedSubsequentState,
                        referencedObject
                    );
                }
            ));

        // TODO(rg): reconsider later.
        if (!task.Objects.Any() && !task.Steps.Any() && req.NewType.HasValue)
        {
            task.Type = req.NewType.Value;
        }

        await JobRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}