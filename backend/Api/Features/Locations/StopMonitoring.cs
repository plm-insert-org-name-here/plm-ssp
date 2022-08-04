using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Services;
using Api.Services.DetectorController;
using Api.Services.MonitoringHandler;
using Api.Services.ProcessorHandler;
using Api.Services.ProcessorHandler.Packets;
using Api.Services.ProcessorHandler.Packets.Req;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Task = System.Threading.Tasks.Task;
using TaskStatus = Api.Domain.Common.TaskStatus;

namespace Api.Features.Locations
{
    public class StopMonitoring : EndpointBaseAsync
        .WithRequest<StopMonitoring.Req>
        .WithActionResult
    {
        private readonly Context _context;
        private readonly MonitoringHandler _monitoringHandler;

        public class Req
        {
            [FromRoute(Name = "id")] public int Id { get; set; }

            [FromBody] public ReqBody Body { get; set; } = default!;

            public record ReqBody(bool Pause);
        }

        public StopMonitoring(Context context, DetectorCommandQueues queues, StreamViewerGroups groups,
            PacketSender sender, MonitoringHandler monitoringHandler)
        {
            _context = context;
            _monitoringHandler = monitoringHandler;
        }

        [HttpPost(Routes.Locations.StopMonitoring)]
        [SwaggerOperation(
            Summary = "Stop monitoring on a location",
            Description = "Stop monitoring on a location",
            OperationId = "Locations.StopMonitoring",
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


            if (location is null) return NotFound();
            if (location.Detector is null)
                return BadRequest("The specified location does not have a detector attached to it");
            if (location.Detector.State is DetectorState.Off)
                return BadRequest("The detector attached to the location is offline");
            if (location.Detector.State is not DetectorState.Monitoring)
                return BadRequest("Monitoring is not active on the specified location");
            if (location.Job is null) return BadRequest("The location has no defined job");

            var activeTask = location.Job.Tasks.SingleOrDefault(t => t.Status is TaskStatus.Active);
            if (activeTask is null) return BadRequest("Active task not found on location");

            await _monitoringHandler.StopMonitoring(location.Detector, activeTask, true);

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}