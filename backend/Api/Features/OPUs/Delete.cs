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

namespace Api.Features.OPUs
{
    public class Delete : EndpointBaseAsync.WithRequest<int>.WithActionResult
    {
        private readonly Context _context;

        public Delete(Context context)
        {
            _context = context;
        }
        

        [HttpDelete(Routes.OPUs.Delete)]
        [SwaggerOperation(
          Summary = "Delete OPU",
          Description = "Delete OPU",
          OperationId = "OPUs.Delete",
          Tags = new[] { "OPUs" }
            )]
        public override async Task<ActionResult> HandleAsync(int id, CancellationToken ct = new())
        {
            var opu = await _context.OPUs
                .Include(o => o.Lines)
                .ThenInclude(l => l.Stations)
                .ThenInclude(s => s.Locations)
                .ThenInclude(l => l.Detector)
                .SingleOrDefaultAsync(s => s.Id == id, ct);

            if (opu is null) return NotFound();
            if (opu.Lines
                .SelectMany(l => l.Stations)
                .SelectMany(s => s.Locations)
                .Any(l => l.Detector?.State is DetectorState.Streaming or DetectorState.Monitoring)
               )
                return BadRequest("A detector on the OPU is busy");

            _context.OPUs.Remove(opu);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}