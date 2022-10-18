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

        [HttpDelete(Routes.Detectors.Delete)]
        [SwaggerOperation(
            Summary = "Delete detector",
            Description = "Delete detector",
            OperationId = "Detectors.Delete",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult> HandleAsync([FromRoute] Req req, CancellationToken ct = new())
        {
            var existingDetector = await _context.Detectors.Where(l => l.Id == req.Id).SingleOrDefaultAsync(ct);

            if (existingDetector is null) return NotFound();
            if (existingDetector.State is not DetectorState.Off)
                return BadRequest("The Detector must be in the Off state for deletion");

            _context.Detectors.Remove(existingDetector);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}