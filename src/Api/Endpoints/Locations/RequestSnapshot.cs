using Application.Services;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Locations;

public class RequestSnapshot : Endpoint<RequestSnapshot.Req>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public IRepository<Task> TaskRepo { get; set; } = default!;
    public DetectorCalibrationHttpService _calibrationService { get; set; } = default!;
    public IRepository<Coordinate> CoordinateRepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.Locations.RequestSnapshot);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
        Description(x =>
        {
            x.Produces(200, null, "application/octet-stream");
        });
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorAndChildrenSpec(req.Id), ct);
        if (location is null || location.Detector is null)
        {
            ThrowError("location or detector is not found");
            return;
        }

        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(req.TaskId), ct);
        if (task is null)
        {
            ThrowError("task is not found");
        }
        
        var result = await _calibrationService.RequestSnapshotAndCoordinates(location.Detector, "normal");
        var calibResult = result.Unwrap();

        location.Snapshot = calibResult.Snapshot;
        await LocationRepo.SaveChangesAsync(ct);

        await CoordinateRepo.DeleteRangeAsync(task.MarkerCoordinates, ct);
        CoordinateRepo.SaveChangesAsync(ct);

        task.MarkerCoordinates = calibResult.Coordinates;
        await TaskRepo.SaveChangesAsync(ct);

        await SendBytesAsync(calibResult.Snapshot, "snapshot.jpeg", "application/octet-stream", null, false, ct);
    }
}