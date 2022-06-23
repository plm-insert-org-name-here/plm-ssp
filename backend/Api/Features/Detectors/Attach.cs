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
    public class Attach : EndpointBaseAsync
        .WithRequest<Attach.Req>
        .WithActionResult
    {
        private readonly Context _context;

        public Attach(Context context)
        {
            _context = context;
        }

        public class Req
        {
            [FromRoute(Name = "id")] public int Id { get; set; }

            [FromBody] public ReqBody Body { get; set; } = default!;

            public record ReqBody(int LocationId);
        }

        [HttpPost(Routes.Detectors.Attach)]
        [SwaggerOperation(
            Summary = "Attach the specified detector to a location",
            Description = "Attach the specified detector to a location",
            OperationId = "Detectors.Attach",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] Req req,
            CancellationToken ct = new())
        {
            var detector = await _context.Detectors
                .Where(d => d.Id == req.Id)
                .Include(d => d.Location)
                .SingleOrDefaultAsync(ct);

            var location = await _context.Locations
                .Where(l => l.Id == req.Body.LocationId)
                .Include(l => l.Detector)
                .SingleOrDefaultAsync(ct);

            if (detector is null) return NotFound();
            if (detector.Location is not null)
                return BadRequest("The specified detector is already assigned to a location");

            if (location is null) return BadRequest("The specified location does not exist");
            if (location.Detector is not null)
                return BadRequest("The specified location is already occupied by a different detector");

            detector.Location = location;
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}