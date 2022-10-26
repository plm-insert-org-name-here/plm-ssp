using Domain.Common;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure.Database;
using Object = Domain.Entities.TaskAggregate.Object;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class Update : Endpoint<Update.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public IRepository<Job> JobRepo { get; set; } = default!;

    public IRepository<Object> ObjectRepo { get; set; } = default!;

    public class Req
    {
        public int ParentJobId {get; set; }
        public int TaskId { get; set; }
        public int LocationId { get; set; }
        public IEnumerable<ReqObject> Objects { get; set; } = default!;
        public record ReqObject(string Name, ObjectCoordinates Coordinates);
        public IEnumerable<ReqSteps> Steps { get; set; } = default!;
        public record ReqSteps(int OrderNum, TemplateState ExpectedInitialState, TemplateState ExpectedSubsequentState, int ObjectId, Object Object);
    }

    public override void Configure()
    {
        Post(Api.Routes.Tasks.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = await JobRepo.FirstOrDefaultAsync(new JobWithTasksSpec(req.ParentJobId), ct);
        var task = job.Tasks.FirstOrDefault(t => t.Id == req.TaskId);

        //TODO: custom exceptions
        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        var location = await LocationRepo.GetByIdAsync(req.LocationId, ct);
        var objects = req.Objects.Select(o => Object.Create(o.Name, o.Coordinates)).ToList();
        var steps = req.Steps.Select((s => Step.Create(s.OrderNum, s.ExpectedInitialState, s.ExpectedSubsequentState, s.ObjectId, s.Object))).ToList();

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        task.Location = location;
        task.Objects = objects;
        task.Steps = steps;

        await JobRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}