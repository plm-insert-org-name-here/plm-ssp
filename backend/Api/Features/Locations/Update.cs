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
    public class Update : EndpointBaseAsync
        .WithRequest<Update.Req>
        .WithActionResult
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

            public record ReqBody(string Name);
        }

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;

            public Validator(Context context)
            {
                _context = context;

                RuleFor(l => l.Body.Name).MaximumLength(64);
                RuleFor(l => l).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
            }

            private bool HaveUniqueName(Req req) =>
                _context.Locations
                    .Where(l => l.Id != req.Id)
                    .All(l => l.Name != req.Body.Name);
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

            existingLocation.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}