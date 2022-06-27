using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Jobs
{
    public class Delete : EndpointBaseAsync
        .WithRequest<Delete.Req>
        .WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }

        public class Req
        {
            [FromRoute(Name = "id")] public int Id { get; set; }
        }

        [HttpDelete(Routes.Jobs.Delete)]
        [SwaggerOperation(
            Summary = "Delete job",
            Description = "Delete job",
            OperationId = "Jobs.Delete",
            Tags = new[] { "Jobs" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] Req req,
            CancellationToken ct = new())
        {
            var job = await _context.Jobs
                .Where(j => j.Id == req.Id)
                .Include(j => j.Location!)
                .ThenInclude(l => l.Detector)
                .SingleOrDefaultAsync(ct);

            if (job is null) return NotFound();
            if (job.Location?.Detector?.State == DetectorState.Running)
                return BadRequest("The detector attached to the location of the job is busy");

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}