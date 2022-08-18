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

namespace Api.Features.Stations
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

                RuleFor(s => s.Body.Name).MaximumLength(64).NotEmpty();
                RuleFor(s => s).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the Line.");
            }

            private bool HaveUniqueNameWithinParent(Req req)
            {
                var station = _context.Stations.SingleOrDefault(l => l.Id == req.Id);
                if (station is null) return false;

                return _context.Stations.Where(s => s.LineId == station.LineId).All(o => o.Name != req.Body.Name);
            }
        }

        [HttpPut(Routes.Stations.Update)]
        [SwaggerOperation(
            Summary = "Update an existing Station",
            Description = "Update an existing Station",
            OperationId = "Stations.Update",
            Tags = new[] { "Stations" })
        ]
        public override async Task<ActionResult> HandleAsync([FromRoute] Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var stations = await _context.Stations.FindAsync(new object[] { req.Id }, ct);
            if (stations is null) return NotFound();

            stations.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}