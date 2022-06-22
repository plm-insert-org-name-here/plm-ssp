using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Services.DetectorController;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Detectors
{
    public class SendCommand : EndpointBaseAsync
        .WithRequest<SendCommand.Req>
        .WithActionResult
    {
        private readonly Context _context;
        private readonly DetectorCommandQueues _queues;

        public class Req
        {
            [FromRoute(Name = "id")] public int Id { get; set; }

            [FromBody] public ReqBody Body { get; set; } = default!;

            public record ReqBody(DetectorCommandType Command);
        }

        public SendCommand(Context context, DetectorCommandQueues queues)
        {
            _context = context;
            _queues = queues;
        }

        [HttpPost(Routes.Detectors.SendCommand)]
        [SwaggerOperation(
            Summary = "Send a command to a detector",
            Description = "Send a command to a detector",
            OperationId = "Detectors.SendCommand",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] Req req,
            CancellationToken ct = new())
        {
            var detector = await _context.Detectors.SingleOrDefaultAsync(d => d.Id == req.Id, ct);

            if (detector is null) return NotFound();
            if (detector.State is DetectorState.Off) return BadRequest("The specified detector is offline");

            var result = _queues.EnqueueCommand(detector.Id, req.Body.Command);
            if (!result) return Problem("Detector is online, but its command queue does not exist");

            return NoContent();
        }
    }
}