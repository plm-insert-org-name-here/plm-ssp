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
    
    // TODO(rg): this is misleading because the .NET Stream type represents an abstract byte stream,
    // while our definition of Stream is a stream of camera images
    // TODO(rg): if multiple clients want to access the stream output of a Detector, the BE should only maintain
    // a single stream connection, which it broadcasts to all clients
    public Task<Stream> RequestStream(Detector detector);
}