using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class DetectorHttpConnection : IDetectorConnection
{
    // all of these methods involve a HTTP client which must be initialized with the detector's IP address.
    // Detectors call the Identify endpoint on startup, and send their MAC and IP addresses
    private readonly IHttpClientFactory _httpClientFactory;

    public DetectorHttpConnection(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task SendCommand(Detector detector, DetectorCommand command)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> RequestSnapshot(Detector detector)
    {
        throw new NotImplementedException();
    }

    public async Task<Stream> RequestStream(Detector detector)
    {
        var client = _httpClientFactory.CreateClient();
        return await client.GetStreamAsync("http://127.0.0.1:3000/stream");
    }
}