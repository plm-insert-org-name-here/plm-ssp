using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Detectors
{
    public class Delete : EndpointBaseAsync.WithRequest<int>.WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }

        [HttpDelete(Routes.Detectors.Delete)]
        [SwaggerOperation(
            Summary = "Delete detector",
            Description = "Delete detector",
            OperationId = "Detectors.Delete",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var existingDetector = await _context.Detectors.Where(l => l.Id == id).SingleOrDefaultAsync(ct);

            if (existingDetector is null) return BadRequest("Detector not found");

            _context.Detectors.Remove(existingDetector);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}