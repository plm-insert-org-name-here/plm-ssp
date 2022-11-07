using System.Net.NetworkInformation;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;
using FormatException = System.FormatException;

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
        var physicalMacAddress = PhysicalAddress.None;

        try
        {
            physicalMacAddress = PhysicalAddress.Parse(req.MacAddress);
        }
        catch (FormatException ex)
        {
            ThrowError(ex.Message);
        }

        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorByMacAddressSpec(physicalMacAddress), ct);
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.LocationId), ct);

        if (location is null)
        {
            ThrowError("Location not found");
            return;
        }

        if (detector is null)
        {
            var newDetector = new Detector(req.MacAddress, physicalMacAddress, req.LocationId, HttpContext.Connection.RemoteIpAddress!);
            await DetectorRepo.AddAsync(newDetector, ct);
            location.AttachDetector(newDetector).Unwrap();

            await LocationRepo.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
            return;
        }

        detector.LocationId = req.LocationId;
        location.AttachDetector(detector).Unwrap();

        await DetectorRepo.SaveChangesAsync(ct);
        await LocationRepo.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}