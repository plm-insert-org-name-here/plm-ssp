using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Jobs;

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Req
    {
        public string Name { get; set; } = default!;

    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

    }
    
    private static Job MapIn(Req r) =>
        new(name: r.Name);

    private static Res MapOut(Job j) =>
        new()
        {
            Id = j.Id,
            Name = j.Name
        };
    
    public override void Configure()
    {
        Post(Api.Routes.Jobs.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Jobs"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var job = MapIn(req);

        await JobRepo.AddAsync(job, ct);

        var res = MapOut(job);
        await SendCreatedAtAsync<Create>(new { job.Id }, res, null, null, false, ct);
    }
}