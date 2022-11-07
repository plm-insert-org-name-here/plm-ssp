using System.Net;

namespace Application.Interfaces;

public interface IDetectorStreamCollection
{
    public void AddStream(IPAddress address, Stream stream);
    public Stream? GetStream(IPAddress address);
}