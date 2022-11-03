using System.Net;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class DetectorHttpConnection : IDetectorConnection
{
    // all of these methods involve a HTTP client which must be initialized with the detector's IP address.
    // Detectors call the Identify endpoint on startup, and send their MAC and IP addresses
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDetectorStreamCollection _detectorStreams;

    public DetectorHttpConnection(IHttpClientFactory httpClientFactory, IDetectorStreamCollection detectorStreams)
    {
        _httpClientFactory = httpClientFactory;
        _detectorStreams = detectorStreams;
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
        // TODO(rg): temporary IP address to localhost; remove this when 
        // detector registration is implemented
        var add = IPAddress.Parse("127.0.0.1");
        detector.IpAddress = add;
        
        // TODO(rg): check IpAddress in caller
        var existingStream = _detectorStreams.GetStream(detector.IpAddress!);

        if (existingStream is not null)
        {
            // TODO(rg): 1st stream request works, but subsequent requests will fail;
            // find out why
            return existingStream;
        }

        var client = _httpClientFactory.CreateClient();
        var stream = await client.GetStreamAsync($"http://{add}:3000/stream");
            
        _detectorStreams.AddStream(detector.IpAddress!, stream);
        return stream;
    }
}