using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class GetHeartBeat : Endpoint<GetHeartBeat.Req, GetHeartBeat.Res>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public class Req 
    {
        public int Id { get; set; }
    }
    
    public class Res
    {
        public IEnumerable<HeartBeatRes> HeartBeats { get; set; } = default!;

        public record HeartBeatRes(
            string MacAddress, int Temperature, int StoragePercentage, long Uptime, float Cpu, float Ram
            );
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Detectors.GetHeartBeat);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorWithHeartBeatsSpec(req.Id), ct);

        if (detector is null)
        {
            ThrowError("Detector not found");
            return;
        }
        
        var res = new Res
        {
            HeartBeats = detector.HeartBeatLogs.Select(log => new Res.HeartBeatRes(detector.MacAddress.ToString(),
                log.Temperature, log.StoragePercentage, log.Uptime, log.Cpu, log.Ram))
        };
        
        await SendOkAsync(res, ct);
    }
}