using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TaskStatus = Domain.Common.TaskStatus;

namespace Application.Features.Tasks
{
    public class Delete : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }

        [HttpDelete(Routes.Tasks.Delete)]
        [SwaggerOperation(
            Summary = "Delete task",
            Description = "Delete task",
            OperationId = "Tasks.Delete",
            Tags = new[] { "Tasks" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] int id,
            CancellationToken ct = new())
        {
            var task = await _context.Tasks
                .SingleOrDefaultAsync(t => t.Id == id, ct);

            if (task is null) return NotFound();
            if (task.Status is TaskStatus.Active)
                return BadRequest("The Task must either be in the Inactive or Paused state");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}