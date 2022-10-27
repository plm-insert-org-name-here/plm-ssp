using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure.Database;
using Domain.Specifications;
using Task = System.Threading.Tasks.Task;

namespace Api.Endpoints.Jobs;

public class GetById: Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }
    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public JobType Type { get; set; }
        public byte[] Snapshot { get; set; } = default!;

        //TODO Location

        public IEnumerable<ResTask> Tasks { get; set; } = default!;

        public record ResTask(int Id, string Name);
    }
    
    private static Res MapOut(Job j) => new()
    {
        Id = j.Id,
        Name = j.Name,
        Type = j.Type,
        Snapshot = j.Snapshot,
        Tasks = j.Tasks.Select(t => new Res.ResTask(t.Id, t.Name)),
    };
    
    public override void Configure()
    {
        Get(Api.Routes.Jobs.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Jobs"));
    }
    
    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = await JobRepo.FirstOrDefaultAsync(new JobWithTasksSpec(req.Id), ct);

        if (job is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(job);
        await SendOkAsync(res, ct);
    }
}