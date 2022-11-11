using System.Net.NetworkInformation;
using Domain.Common;
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
    public IDetectorConnection DetectorConnection { get; set; } = default!;

    // TODO(rg): save calibration coords to database
    public class Req
    {
        public int LocationId { get; set; }
        public string MacAddress { get; set; } = default!;
        public List<CalibrationCoordsReq> Coordinates { get; set; } = default!;

        public record CalibrationCoordsReq(int X, int Y);
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

        // Endpoint is always called through TCP, so the IP address is never null
        var remoteIpAddress = HttpContext.Connection.RemoteIpAddress!;

        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorByMacAddressSpec(physicalMacAddress), ct);
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.LocationId), ct);

        if (location is null)
        {
            ThrowError("Location not found");
            return;
        }

        if (detector is null)
        {
            var newDetector = Detector.Create(req.MacAddress, physicalMacAddress, remoteIpAddress, location).Unwrap();
            await DetectorRepo.AddAsync(newDetector, ct);
            detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorByMacAddressSpec(physicalMacAddress), ct);
        }
        else
        {
            location.AttachDetector(detector).Unwrap();
        }

        var originalCoords =
            location.SetNewCoordinates(req.Coordinates.Select(c => new CalibrationCoordinates(c.X, c.Y)).ToList());
        if (!originalCoords.Any())
        {
            if (originalCoords.All(c => c.IsValid()))
            {
                var result = detector?.SendRecalibrate(originalCoords, DetectorConnection);
                result?.Unwrap();
            }
            ThrowError("some of the coordinates are not valid!");
            return;
        }

        await DetectorRepo.SaveChangesAsync(ct);
        await LocationRepo.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}