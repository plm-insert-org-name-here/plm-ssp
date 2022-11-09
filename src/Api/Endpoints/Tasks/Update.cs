using Domain.Common;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Object = Domain.Entities.TaskAggregate.Object;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class Update : Endpoint<Update.Req, Update.Res>
{
    public IRepository<Job> JobRepo { get; set; } = default!;

    public IRepository<Task> TaskRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public int ParentJobId {get; set; }
        public IEnumerable<NewObjectReq> NewObjects { get; set; } = default!;
        public IEnumerable<ModObjectReq> ModifiedObjects { get; set; } = default!;
        public List<int> DeletedObjects { get; set; } = default!;
        public record NewObjectReq(string Name, ObjectCoordinates Coordinates);
        public record ModObjectReq(int Id, string Name, ObjectCoordinates Coordinates);

        public IEnumerable<NewStepReq> NewSteps { get; set; } = default!;
        public IEnumerable<ModStepReq> ModifiedSteps { get; set; } = default!;
        public List<int> DeletedSteps { get; set; } = default!;
        public record NewStepReq(int OrderNum, TemplateState ExpectedInitialState, TemplateState ExpectedSubsequentState, int ObjectId);
        public record ModStepReq(int Id, int OrderNum, TemplateState ExpectedInitialState, TemplateState ExpectedSubsequentState, int ObjectId);

        public string NewName { get; set; } = default!;
        public TaskType NewType { get; set; }
    }

    public class Res
    {
        public ResTask Task { get; set; } = default!;
        public List<NewStepRes> Steps { get; set; } = default!;

        public record NewStepRes(int? OrderNum, TemplateState ExpectedInitialState, TemplateState ExpectedSubsequentState, int ObjectId);
        public record ResTask(int Id, int ParentJobId, string Name, TaskType Type, int NumObjects, int NumSteps);
    }

    public Res MapOut(Task task, List<Step> steps) => new()
    {
        Task = new Res.ResTask(task.Id, task.JobId, task.Name, task.Type, task.Objects.Count, task.Steps.Count),
        Steps = steps.Select(s => new Res.NewStepRes(s.OrderNum, s.ExpectedInitialState, s.ExpectedSubsequentState, s.ObjectId)).ToList()
    };

    public override void Configure()
    {
        Put(Api.Routes.Tasks.Update);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = await JobRepo.FirstOrDefaultAsync(new JobWithTasksSpec(req.ParentJobId), ct);
        if (job is null)
        {
            ThrowError("Parent Job does not exist");
            return;
        }

        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(req.Id), ct);
        if (task is null || task.JobId != job.Id)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // TODO: check if the name is unique within parent job
        task.Name = req.NewName;
        if (!task.Objects.Any() && !task.Steps.Any())
        {
            task.Type = req.NewType;
        }

        // TODO: objects' names within task should be unique
        // TODO: steps should refer to objects by their names and not by ID, because on object creation, their ID is unknown until the DB roundtrip

        // check if steps' object belongs to this task
        if (!req.NewSteps.Any() && !req.NewSteps.All(s => task.IsObjectBelongsTo(s.ObjectId)))
        {
            ThrowError("Some Objects do not belong to this Task");
            return;
        }
        
        if (!req.ModifiedSteps.Any() && !req.ModifiedSteps.All(s => task.IsObjectBelongsTo(s.ObjectId)))
        {
            ThrowError("Some Objects do not belong to this Task");
            return;
        }

        //delete
        task.Objects.RemoveAll(o => req.DeletedObjects.Contains(o.Id));
        task.Steps.RemoveAll(s => req.DeletedSteps.Contains(s.Id));

        // TODO: modify
        // task.Objects = task.Objects.Where(o => req.ModifiedObjects.Select(m => m.Id).Contains(o.Id)).ToList();
        // task.Steps = task.Steps.Where(s => req.ModifiedSteps.Select(m => m.Id).Contains(s.Id)).ToList();

        //add
        task.Objects.AddRange(req.NewObjects.Select(o => Object.Create(o.Name, o.Coordinates)));
        task.Steps.AddRange(req.NewSteps.Select(s => Step.Create(s.OrderNum, s.ExpectedInitialState, s.ExpectedSubsequentState, s.ObjectId, req.Id)));
        
        await JobRepo.SaveChangesAsync(ct);
        
        var res = MapOut(task, task.Steps);
        await SendOkAsync(res, ct);
    }
}