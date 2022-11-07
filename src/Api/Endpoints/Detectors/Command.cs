using Domain.Common;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class Command : Endpoint<Command.Req, EmptyResponse>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public DetectorCommand Command { get; set; } = default!;
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

        if (detector.Location is null &&
            (req.Command.IsStartDetection ||
             req.Command.IsPauseDetection ||
             req.Command.IsResumeDetection ||
             req.Command.IsStopDetection))
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
                if (req.Command.IsPauseDetection || req.Command.IsStopDetection || req.Command.IsResumeDetection)
                    ThrowError("Inactive tasks can't be stopped, resumed or paused");
            }
            else if (ongoingTask.State is TaskState.Paused)
            {
                if (req.Command.IsPauseDetection || req.Command.IsStartDetection)
                    ThrowError("Paused tasks can't be started or paused");
            }
            else if (ongoingTask.State is TaskState.Active)
            {
                if (req.Command.IsStartDetection || req.Command.IsResumeDetection)
                    ThrowError("Active tasks can't be started or resumed");
            }
        }

        var result = await DetectorConnection.SendCommand(detector, req.Command);
        result.Unwrap();

        await SendNoContentAsync(ct);
    }
}