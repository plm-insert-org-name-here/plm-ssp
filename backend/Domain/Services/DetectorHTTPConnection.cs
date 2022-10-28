using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public class DetectorHTTPConnection : IDetectorConnection
{
    // TODO(rg): HTTP client
    // all of these methods involve a HTTP client which must be initialized with the detector's IP address.
    // Detectors call the Identify endpoint on startup, and send their MAC and IP addresses
    
    public Task SendCommand(Detector detector, DetectorCommand command)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> RequestSnapshot(Detector detector)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> RequestStream(Detector detector)
    {
        throw new NotImplementedException();
    }
}