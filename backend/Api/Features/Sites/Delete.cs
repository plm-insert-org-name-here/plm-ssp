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

namespace Api.Features.Sites
{
    public class Delete : EndpointBaseAsync.WithRequest<int>.WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }
        

        [HttpDelete(Routes.Sites.Delete)]
        [SwaggerOperation(
          Summary = "Delete site",
          Description = "Delete site",
          OperationId = "Sites.Delete",
          Tags = new[] { "Sites" }
            )]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var site = await _context.Sites
                .Include(s => s.OPUs)
                .ThenInclude(o => o.Lines)
                .ThenInclude(l => l.Stations)
                .ThenInclude(s => s.Locations)
                .ThenInclude(l => l.Detector)
                .SingleOrDefaultAsync(s => s.Id == id, ct);

            if (site is null) return NotFound();
            if (site.OPUs
                .SelectMany(o => o.Lines)
                .SelectMany(l => l.Stations)
                .SelectMany(s => s.Locations)
                .Any(l => l.Detector?.State is DetectorState.Streaming or DetectorState.Monitoring)
               )
                return BadRequest("A detector on the site is busy");

            _context.Sites.Remove(site);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}