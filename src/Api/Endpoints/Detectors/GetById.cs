using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string MacAddress { get; set; } = default!;
        public string IpAddress { get; set; } = default!;
        public DetectorState State { get; set; }
        public ResLocation? Location { get; set; }

        public record ResLocation(int Id, string Name);
    }

    private static Res MapOut(Detector d)
    {
        var res = new Res
        {
            Id = d.Id,
            Name = d.Name,
            MacAddress = d.MacAddress.ToString(),
            IpAddress = d.IpAddress.ToString(),
            State = d.State
        };

        if (d.Location is not null)
            res.Location = new Res.ResLocation(d.Location.Id, d.Location.Name);

        return res;
    }

    public override void Configure()
    {
        Get(Api.Routes.Detectors.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorByIdWithLocationSpec(req.Id), ct);

        if (detector is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(MapOut(detector), ct);
    }
}