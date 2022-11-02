using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class Command : Endpoint<Command.Req, EmptyResponse>
{
    public readonly IRepository<Detector> DetectorRepo = default!;
    public readonly IDetectorConnection DetectorConnection = default!;
    
    public class Req
    {
        public int Id { get; set; }
        public DetectorCommand Command { get; set; }
    }
    
    public override void Configure()
    {
        Post(Api.Routes.Detectors.Command);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }
    
    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var detector = await DetectorRepo.FirstOrDefaultAsync(new DetectorWithLocationAndTasks(req.Id), ct);

        if (detector is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (detector.State is DetectorState.Off)
        {
            ThrowError("Detector is offline");
            return;
        }

        if (detector.Location is null && req.Command is
            DetectorCommand.StartDetection or
            DetectorCommand.PauseDetection or
            DetectorCommand.StopDetection)
        {
            ThrowError("Detection commands can't be sent to detached detectors");
            return;
        }

        if (detector.Location is not null)
        {
            var ongoingTask = detector.Location.Tasks
                .FirstOrDefault(t => t.State is TaskState.Active or TaskState.Paused);

            if (ongoingTask is null)
            {
                if (req.Command is DetectorCommand.PauseDetection or DetectorCommand.StopDetection)
                    ThrowError("Inactive tasks can't be paused or stopped");
            }
            else
            {
                if (ongoingTask.State is TaskState.Inactive && req.Command is DetectorCommand.PauseDetection)
                    ThrowError("Paused tasks can't be paused");
                else if (ongoingTask.State is TaskState.Active && req.Command is DetectorCommand.StartDetection)
                    ThrowError("Active tasks can't be started");
            }
        }
        
        await DetectorConnection.SendCommand(detector, req.Command);
        
        await SendNoContentAsync(ct);
    }
}