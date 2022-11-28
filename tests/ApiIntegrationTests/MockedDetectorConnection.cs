using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using FluentResults;

namespace ApiIntegrationTests;

public class MockedDetectorConnection : IDetectorConnection
{
    public Task<Result> SendCommand(Detector detector, DetectorCommand command)
    {
        return Task.FromResult(Result.Ok());
    }

    public Task<Result<byte[]>> RequestSnapshot(Detector detector)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Stream>> RequestStream(Detector detector)
    {
        throw new NotImplementedException();
    }
}