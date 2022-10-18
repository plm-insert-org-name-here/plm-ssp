using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Detectors
{
    public class Detach : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult
    {
        private readonly Context _context;

        public Detach(Context context)
        {
            _context = context;
        }

        [HttpPost(Routes.Detectors.Detach)]
        [SwaggerOperation(
            Summary = "Detach the specified detector from a location",
            Description = "Detach the specified detector from a location",
            OperationId = "Detectors.Detach",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var detector = await _context.Detectors
                .Where(d => d.Id == id)
                .Include(d => d.Location)
                .SingleOrDefaultAsync(ct);

            if (detector is null) return NotFound();
            if (detector.Location is null)
                return BadRequest("The specified detector is not attached to a location");
            if (detector.State is DetectorState.Streaming or DetectorState.Monitoring)
                return BadRequest("The specified detector is busy");

            detector.Location = null;
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}