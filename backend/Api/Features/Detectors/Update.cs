using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Detectors
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

                RuleFor(d => d.Body.Name).MaximumLength(64);
                RuleFor(d => d).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
            }

            private bool HaveUniqueName(Req req) =>
                _context.Detectors
                    .Where(d => d.Id != req.Id)
                    .All(d => d.Name != req.Body.Name);
        }

        [HttpPut(Routes.Detectors.Update)]
        [SwaggerOperation(
            Summary = "Update an existing detector",
            Description = "Update an existing detector",
            OperationId = "Detectors.Update",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] Req req,
            CancellationToken ct = new())
        {
            var existingDetector = await _context.Detectors.FindAsync(new object[] { req.Id }, ct);
            if (existingDetector is null) return BadRequest("Detector does not exist");

            existingDetector.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

    }
}