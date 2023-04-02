using Api.Endpoints.Detectors;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Domain.Specifications;
using Infrastructure;
using Infrastructure.Logging;

namespace Api.Endpoints.CAA;

public interface IMockCAA
{
    public int Iterations { get; set; }
    public int Period { get; set; }
    public DetectorCommandService CommandService { get; set; }
    public bool Active { get; set; }

    public void Start(int period,Detector detector, int taskId);
    public int Stop();
}

public class MockCAA : IMockCAA
{
    public int Iterations { get; set; }
    public int Period { get; set; }
    
    public DetectorCommandService CommandService { get; set; } = default!;
    public bool Active { get; set; }

    public void Start(int period,Detector detector, int taskId)
    {
        Period = period;
        Active = true;
        TriggerIter(detector, taskId);
    }

    public int Stop()
    {
        Active = false;
        return Iterations;
    }

    private async Task TriggerIter(Detector detector, int taskId)
    {
        var ct = new CancellationToken();

        var command = new DetectorCommand.StartDetection(taskId);

        while (Active)
        {
            PlmLogger.Log("MockCaa - while");
            Iterations += 1;
            var result = await CommandService.HandleCommand(detector, command, ct);
            result.Unwrap();
            PlmLogger.Log("MockCaa Detection - " + Iterations);
            Thread.Sleep(Period * 1000 * 60);
        }
        PlmLogger.Log("MockCaa - " + Iterations);
        Iterations = 0;
    }
}