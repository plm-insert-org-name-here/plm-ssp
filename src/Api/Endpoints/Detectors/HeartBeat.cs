using System.Net.NetworkInformation;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class HeartBeat : Endpoint<HeartBeat.Req, EmptyResponse>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public class Req
    {
        public string MacAddress { get; set; } = default!;
        public int Temperature { get; set; }
        public int FreeStoragePercentage { get; set; }
        public int Uptime { get; set; }
    }

    public override void Configure()
    {
        Post(Api.Routes.Detectors.HeartBeat);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var physicalMacAddress = PhysicalAddress.Parse(req.MacAddress);
        var remoteIpAddress = HttpContext.Connection.RemoteIpAddress!;
        
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorByMacAddressSpec(PhysicalAddress.Parse(req.MacAddress)), ct);
        if (detector is null)
        {
            detector = Detector.Create(req.MacAddress, physicalMacAddress, remoteIpAddress, null).Unwrap();
        }

        var newLog = new Detector.HeartBeatLog
        {
            Temperature = req.Temperature,
            FreeStoragePercentage = req.FreeStoragePercentage,
            Uptime = req.Uptime
        };

        detector.HeartBeatLogs.Add(newLog);
        await DetectorRepo.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }
}