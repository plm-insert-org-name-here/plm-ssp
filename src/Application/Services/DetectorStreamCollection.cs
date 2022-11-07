using System.Net;
using Application.Interfaces;

namespace Application.Services;

public class DetectorStreamCollection : IDetectorStreamCollection
{
    private readonly Dictionary<IPAddress, Stream> _streams = new();

    public void AddStream(IPAddress address, Stream stream)
    {
        _streams[address] = stream;
    }

    public Stream? GetStream(IPAddress address)
    {
        return _streams.ContainsKey(address) ? _streams[address] : null;
    }
}