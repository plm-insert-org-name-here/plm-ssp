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

namespace Api.Features.Lines
{
    public class Delete : EndpointBaseAsync.WithRequest<int>.WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }
        

        [HttpDelete(Routes.Lines.Delete)]
        [SwaggerOperation(
          Summary = "Delete Line",
          Description = "Delete Line",
          OperationId = "Lines.Delete",
          Tags = new[] { "Lines" }
            )]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var line = await _context.Lines
                .Include(l => l.Stations)
                .ThenInclude(s => s.Locations)
                .ThenInclude(l => l.Detector)
                .SingleOrDefaultAsync(s => s.Id == id, ct);

            if (line is null) return NotFound();
            if (line.Stations
                .SelectMany(s => s.Locations)
                .Any(l => l.Detector?.State is DetectorState.Streaming or DetectorState.Monitoring)
               )
                return BadRequest("A detector on the Line is busy");

            _context.Lines.Remove(line);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}