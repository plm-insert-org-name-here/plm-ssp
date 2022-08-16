using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Infrastructure.Database;
using Api.Services.ProcessorHandler;
using Api.Services.ProcessorHandler.Packets.Req;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using TaskStatus = Api.Domain.Common.TaskStatus;

namespace Api.Features.Tasks
{
    public class AbandonInstance : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult
    {
        private readonly Context _context;
        private readonly PacketSender _sender;

        public AbandonInstance(Context context, PacketSender sender)
        {
            _context = context;
            _sender = sender;
        }

        [HttpPost(Routes.Tasks.AbandonInstance)]
        [SwaggerOperation(
            Summary = "Abandon a paused task instance",
            Description = "Abandon a paused task instance",
            OperationId = "Tasks.ResetInstance",
            Tags = new[] { "Tasks" })
        ]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var task = await _context.Tasks
                .Include(t => t.TaskInstances)
                .SingleOrDefaultAsync(t => t.Id == id, ct);

            if (task is null)
                return NotFound();
            if (task.Status is TaskStatus.Active)
                return BadRequest("The task currently is active");

            var instance = task.TaskInstances.SingleOrDefault(ti => ti.FinalState == null);

            if (instance is null)
                return BadRequest("The task does not have any unfinished instances");

            await _sender.SendPacket(new StopPacket(task.Id));
            instance.FinalState = TaskInstanceFinalState.Abandoned;
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}