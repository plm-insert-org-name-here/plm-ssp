using System.Net.NetworkInformation;
using Api.Endpoints.Lines;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class Identify : Endpoint<Identify.Req, EmptyResponse>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public class Req
    {
        public int LocationId { get; set; }
        public string  MacAddress { get; set; } = default!;
    }

    public override void Configure()
    {
        Post(Api.Routes.Detectors.Identify);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var phisicalMacAddress = PhysicalAddress.Parse(req.MacAddress);

        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorByMacAddressSpec(phisicalMacAddress), ct);
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.LocationId), ct);

        if (location is null)
        {
            //TODO
            await SendNotFoundAsync(ct);
            return;
        }
        
        if (detector is null)
        {
            var newDetector = new Detector(req.MacAddress, phisicalMacAddress, req.LocationId, HttpContext.Connection.RemoteIpAddress);
            await DetectorRepo.AddAsync(newDetector, ct);
            var result = location.AttachDetector(newDetector);
            await LocationRepo.SaveChangesAsync(ct);
            
            await SendStringAsync(result.ToString());
            return;
        }
        
        detector.LocationId = req.LocationId;
        var result2 = location.AttachDetector(detector);
        await DetectorRepo.SaveChangesAsync(ct);
        await LocationRepo.SaveChangesAsync(ct);
        
        await SendStringAsync(result2.ToString());
        return;
    }
}