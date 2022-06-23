using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Services;
using Ardalis.ApiEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Jobs
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult
    {
        private readonly Context _context;
        private readonly LocationSnapshotCache _snapshotCache;

        public Create(Context context, LocationSnapshotCache snapshotCache)
        {
            _context = context;
            _snapshotCache = snapshotCache;
        }

        public record Req(string Name, JobType Type, int LocationId);

        public class Validator : AbstractValidator<Req>
        {
            public Validator()
            {
                RuleFor(j => j.Name).MaximumLength(64);
            }
        }

        [HttpPost(Routes.Jobs.Create)]
        [SwaggerOperation(
            Summary = "Create new job",
            Description = "Create new job",
            OperationId = "Jobs.Create",
            Tags = new[] { "Jobs" })
        ]
        public override async Task<ActionResult> HandleAsync(Req req, CancellationToken ct = new())
        {
            var location = await _context.Locations
                .Where(l => l.Id == req.LocationId)
                .Include(l => l.Job)
                .SingleOrDefaultAsync(ct);


            if (location is null) return NotFound();
            if (location.Job is not null)
                return BadRequest("The specified location already has a Job associated with it");

            var snapshot = _snapshotCache.Get(location.Id);
            if (snapshot is null)
                return BadRequest("No snapshot has been yet taken on the location");

            // TODO(rg)

            return NoContent();
        }
    }
}