using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Jobs;

public class List: EndpointWithoutRequest<IEnumerable<List.Res>>
{    
    public IRepository<Job> JobRepo { get; set; } = default!;

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Res MapOut(Job j) =>
        new()
        {
            Id = j.Id,
            Name = j.Name
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