using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class CollectData : Endpoint<CollectData.Req, CollectData.Res>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    
    public IDetectorConnection DetectorConnection { get; set; }= default!;
    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public byte[] File { get; set; }
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Detectors.CollectData);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {

        var detector = await DetectorRepo.GetByIdAsync(req.Id, ct);

        if (detector is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        //check if the dector is streaming
        if ((detector.State & DetectorState.Streaming) == DetectorState.Streaming)
        {
            ThrowError("Detector is streaming!");
        }
        
        var result = await DetectorConnection.RequestCollectData(detector);
        var file = result.Unwrap();

        await SendBytesAsync(file, cancellation: ct, fileName:"data.zip");
    }
}