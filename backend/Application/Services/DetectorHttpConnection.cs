using System.Data;
using System.Net;
using System.Text.Json;
using Application.Interfaces;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using FluentResults;

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

    public async Task<Result> SendCommand(Detector detector, DetectorCommand command)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(command);

            var response = await client.PostAsync("http://127.0.0.1:3000/command", new StringContent(json));
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }

    public async Task<Result<byte[]>> RequestSnapshot(Detector detector)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var snapshot = await client.GetByteArrayAsync("http://127.0.0.1:3000/snapshot");

            return snapshot;
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<Stream>> RequestStream(Detector detector)
    {
        try
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
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}