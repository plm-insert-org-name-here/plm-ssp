using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Stations
{
    public class Delete : EndpointBaseAsync.WithRequest<int>.WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }
        

        [HttpDelete(Routes.Stations.Delete)]
        [SwaggerOperation(
          Summary = "Delete Station",
          Description = "Delete Station",
          OperationId = "Stations.Delete",
          Tags = new[] { "Stations" }
            )]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var station = await _context.Stations
                .Include(s => s.Locations)
                .ThenInclude(l => l.Detector)
                .SingleOrDefaultAsync(s => s.Id == id, ct);

            if (station is null) return NotFound();
            if (station.Locations.Any(l => l.Detector?.State is DetectorState.Streaming or DetectorState.Monitoring))
                return BadRequest("A detector on the Station is busy");

            _context.Stations.Remove(station);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}