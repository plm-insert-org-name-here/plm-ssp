using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var existingLocation = await _context.Locations.Where(l => l.Id == id).SingleOrDefaultAsync(ct);

            if (existingLocation is null) return BadRequest("Location not found");

            _context.Locations.Remove(existingLocation);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}