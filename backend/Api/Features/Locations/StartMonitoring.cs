using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Services.DetectorController;
using Api.Services.ProcessorHandler;
using Api.Services.ProcessorHandler.Packets;
using Api.Services.ProcessorHandler.Packets.Req;
using Ardalis.ApiEndpoints;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using TaskStatus = Api.Domain.Common.TaskStatus;

namespace Api.Features.Locations
{
    public class StartMonitoring : EndpointBaseAsync
        .WithRequest<StartMonitoring.Req>
        .WithActionResult
    {
        private readonly Context _context;
        private readonly DetectorCommandQueues _queues;
        private readonly IConfigurationProvider _mapperConfig;
        private readonly PacketSender _sender;
        private readonly ILogger _logger;

        public class Req
        {
            [FromRoute(Name = "id")] public int Id { get; set; }

            [FromBody] public ReqBody Body { get; set; } = default!;

            public record ReqBody(int TaskId);
        }

        public StartMonitoring(Context context, DetectorCommandQueues queues, IConfigurationProvider mapperConfig, PacketSender sender, ILogger logger)
        {
            _context = context;
            _queues = queues;
            _mapperConfig = mapperConfig;
            _sender = sender;
            _logger = logger;
        }

        [HttpPost(Routes.Locations.StartMonitoring)]
        [SwaggerOperation(
            Summary = "Start monitoring on a location",
            Description = "Start monitoring on a location",
            OperationId = "Locations.StartMonitoring",
            Tags = new[] { "Locations" })
        ]
        public override async Task<ActionResult> HandleAsync([FromRoute] Req req, CancellationToken ct = new())
        {
            var location = await _context.Locations
                .Where(l => l.Id == req.Id)
                .Include(l => l.Detector)
                .Include(l => l.Job!)
                .ThenInclude(j => j.Tasks)
                .SingleOrDefaultAsync(ct);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == req.Body.TaskId, ct);

            if (location is null) return NotFound();
            if (location.Detector is null) return BadRequest("The specified location does not have a detector attached to it");

            var originalState = location.Detector.State;

            if (originalState is DetectorState.Off)
                return BadRequest("The detector attached to the location is offline");
            if (originalState is DetectorState.Monitoring)
                return BadRequest("Monitoring is already in progress on the specified location");
            if (location.Job is null) return BadRequest("The location has no defined job");

            if (task is null) return BadRequest("The specified task does not exist");
            if (location.Job.Tasks.All(t => t.Id != req.Body.TaskId))
                return BadRequest("The specified task is not associated with this location");

            task.Status = TaskStatus.Active;
            location.Detector.State = DetectorState.Monitoring;
            await _context.SaveChangesAsync(ct);

            var taskInstance = await _context.TaskInstances
                .SingleOrDefaultAsync(ti => ti.Task.Id == task.Id && ti.FinalState == null, ct);

            if (taskInstance is null)
            {
                taskInstance = new TaskInstance
                {
                    Events = new List<Event>(),
                    Task = task
                };

                await _context.TaskInstances.AddAsync(taskInstance, ct);
                await _context.SaveChangesAsync(ct);
            }

            var processorParams = await _context.Tasks
                .Where(t => t.Id == req.Body.TaskId)
                .Include(t => t.Job)
                .ThenInclude(j => j.Location!)
                .ThenInclude(l => l.Detector)
                .Include(t => t.Templates)
                .ProjectTo<ParamsPacket>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            await _sender.SendPacket(processorParams);

            if (originalState is DetectorState.Standby)
                _queues.EnqueueCommand(location.Detector.Id, DetectorCommandType.StartStreaming);

            return NoContent();
        }
    }
}