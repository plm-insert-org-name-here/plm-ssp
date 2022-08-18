using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Database;
using Api.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Locations
{
    public class Update : EndpointBaseAsync
        .WithRequest<Update.Req>
        .WithActionResult
    {
        private readonly Context _context;
        private readonly IValidator<Req> _validator;

        public Update(Context context, IValidator<Req> validator)
        {
            _context = context;
            _validator = validator;
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

                RuleFor(l => l.Body.Name).MaximumLength(64).NotEmpty();
                RuleFor(l => l).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the Station.");
            }

            private bool HaveUniqueNameWithinParent(Req req)
            {
                var location = _context.Locations.SingleOrDefault(l => l.Id == req.Id);
                if (location is null) return false;

                return _context.Locations
                    .Where(l => l.StationId == location.StationId)
                    .All(l => l.Name != req.Body.Name);
            }
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
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var existingLocation = await _context.Locations.FindAsync(new object[] { req.Id }, ct);
            if (existingLocation is null) return NotFound();

            existingLocation.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}