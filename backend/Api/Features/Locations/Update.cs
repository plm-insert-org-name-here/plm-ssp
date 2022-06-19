using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Locations
{
    public class Update : EndpointBaseAsync.WithRequest<Update.Req>.WithActionResult
    {
        private readonly Context _context;

        public Update(Context context)
        {
            _context = context;
        }

        public class Req
        {
            [FromRoute(Name = "id")] public int Id { get; set; }
            [FromBody] public ReqBody Body { get; set; } = default!;

            public class ReqBody
            {
                public string Name { get; set; } = default!;
            }
        }

        public class Validator : AbstractValidator<Req>
        {
            public Validator() => RuleFor(l => l.Body.Name).Length(8, 64);
        }

        [HttpPut(Routes.Locations.Update)]
        [SwaggerOperation(
            Summary = "Update an existing location",
            Description = "Update an existing location",
            OperationId = "Locations.Update",
            Tags = new[] { "Locations" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] Req req,
            CancellationToken ct = new())
        {
            var existingLocation = await _context.Locations.FindAsync(new object[] { req.Id }, ct);

            if (existingLocation is null) return BadRequest("Location does not exist");

            if (_context.Locations
                .Where(l => l.Id != req.Id)
                .Any(l => l.Name == req.Body.Name))
            {
                return BadRequest("Location with the same name already exists");
            }

            existingLocation.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();

        }
    }
}