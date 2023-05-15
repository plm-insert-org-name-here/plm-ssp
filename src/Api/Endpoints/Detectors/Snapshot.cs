using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.TaskAggregate;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Namotion.Reflection;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Detectors;

public class Snapshot : Endpoint<Snapshot.Req>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; }= default!;
    public IRepository<Task> TaskRepo { get; set; } = default!;
    public DetectorCalibrationHttpService _calibrationService { get; set; } = default!;
    public IRepository<Coordinate> CoordinateRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int TaskId { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.Detectors.Snapshot);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }
    
    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorWithLocationAndTasksSpec(req.Id), ct);
        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(req.TaskId), ct);
        
        if (detector is null || task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (detector.State is DetectorState.Off)
        {
            ThrowError("Detector is offline");
            return;
        }

        if (detector.Location is null)
        {
            ThrowError("Snapshots can't be requested from detached detectors");
            return;
        }

        var result = await _calibrationService.RequestSnapshotAndCoordinates(detector, req.Type);
        var calibResult = result.Unwrap();

        detector.Location.Snapshot = calibResult.Snapshot;
        await DetectorRepo.SaveChangesAsync(ct);

        task.MarkerCoordinates = calibResult.Coordinates;
        await TaskRepo.SaveChangesAsync(ct);
        
        await SendBytesAsync(calibResult.Snapshot, cancellation: ct);
    }
}
