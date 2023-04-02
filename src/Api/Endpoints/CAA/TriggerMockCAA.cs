using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure.Logging;

namespace Api.Endpoints.CAA;

public class TriggerMockCAA : Endpoint<TriggerMockCAA.Req, TriggerMockCAA.Res>
{
    public IMockCAA MockCAA { get; set; } = default!;
    public IRepository<Detector> DetectorRepo { get; set; } = default!;

    public class Req
    {
        public int? Period { get; set; }
    }
    
    public class Res
    {
        public int? Iterations { get; set; }
    }
    
    public override void Configure()
    {
        Post(Api.Routes.CAA.TriggerMock);
        AllowAnonymous();
        Options(x => x.WithTags("CAA"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detectorId = 11;
        var taskId = 19;
        
        int? iters = null;
        
        var detector = await DetectorRepo.GetByIdAsync(detectorId);

        if (detector is null)
        {
            PlmLogger.Log("MockCaa - detector is null");
            ThrowError("detector is null");
            return;
        }

        if (req.Period != null)
        {
            PlmLogger.Log("MockCaa start");
            MockCAA.Start(req.Period.Value, detector, taskId);
        }
        else
        {
            PlmLogger.Log("MockCaa stop");
            iters = MockCAA.Stop();
        }

        var res = new Res
        {
            Iterations = 0
        };

        await SendOkAsync(res, ct);
    }
}