using Domain.Common;
using Domain.Entities;
using FastEndpoints;
using Infrastructure.Database;

namespace Api.Endpoints.Jobs;

public class List: EndpointWithoutRequest<IEnumerable<List.Res>>
{    
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public JobType Type { get; set; }
        public byte[] Snapshot { get; set; } = default!;
    }

    private static Res MapOut(Job j) =>
        new()
        {
            Id = j.Id,
            Name = j.Name,
            Type = j.Type,
            Snapshot = j.Snapshot
        };

    public override void Configure()
    {
        Get(Api.Routes.Jobs.List);
        AllowAnonymous();
        Options(x => x.WithTags("Jobs"));
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var jobs = await JobRepo.ListAsync(ct);

        await SendOkAsync(jobs.Select(MapOut), ct);
    }
}