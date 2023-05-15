using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Locations;

public class GetTasks : Endpoint<GetTasks.Req, GetTasks.Res>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public IEnumerable<TaskRes> Tasks { get; set; } = default!;

        public record TaskRes(int Id, string Name, string JobName, bool Active);
    }

    private static Res MapOut(Location location) => new()
    {
        Tasks = location.Tasks.Select(t => new Res.TaskRes(t.Id, t.Name, t.Job.Name, t.OngoingInstance is not null))
    };

    public override void Configure()
    {
        Get(Api.Routes.Locations.GetTasks);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithTasksSpec(req.Id), ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(location);
        await SendOkAsync(res, ct);
    }
}