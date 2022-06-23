using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Locations
{
    public class Delete : EndpointBaseAsync.WithRequest<int>.WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }

        [HttpDelete(Routes.Locations.Delete)]
        [SwaggerOperation(
            Summary = "Delete location",
            Description = "Delete location",
            OperationId = "Locations.Delete",
            Tags = new[] { "Locations" })
        ]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var existingLocation = await _context.Locations
                .Where(l => l.Id == id)
                .Include(l => l.Detector)
                .SingleOrDefaultAsync(ct);

            if (existingLocation is null) return NotFound();
            if (existingLocation.Detector?.State == DetectorState.Running)
                return BadRequest("The detector attached to the location is busy");

            _context.Locations.Remove(existingLocation);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}