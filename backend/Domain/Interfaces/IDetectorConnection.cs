using Domain.Common;
using Domain.Entities;

namespace Domain.Interfaces;

// NOTE(rg): all methods take a Detector argument because the implementors might need properties of the Detector
// (e.g. HTTP implementation needs Detector.IPAddress)
public interface IDetectorConnection
{
    public Task SendCommand(Detector detector, DetectorCommand command);
    
    // TODO(rg): wrap byte[] into a more descriptive object (e.g. Image / Frame / Snapshot)
    public Task<byte[]> RequestSnapshot(Detector detector);
    
    public Task<Stream> RequestStream(Detector detector);
}