using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Jobs
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

            public record ReqBody(string Name, JobType? Type);
        }

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;

            public Validator(Context context)
            {
                _context = context;

                RuleFor(j => j.Body.Name).NotEmpty().MaximumLength(64);
                RuleFor(j => j).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
                RuleFor(j => j.Body.Type).NotNull();
            }

            private bool HaveUniqueName(Req req) =>
                _context.Jobs
                    .Where(j => j.Id != req.Id)
                    .All(j => j.Name != req.Body.Name);
        }

        [HttpPut(Routes.Jobs.Update)]
        [SwaggerOperation(
            Summary = "Update an existing job",
            Description = "Update an existing job",
            OperationId = "Jobs.Update",
            Tags = new[] { "Jobs" })
        ]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute] Req req,
            CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var job = await _context.Jobs
                .Where(j => j.Id == req.Id)
                .Include(j => j.Location!)
                .ThenInclude(l => l.Detector)
                .SingleOrDefaultAsync(ct);

            if (job is null) return NotFound();
            if (job.Location?.Detector?.State == DetectorState.Running)
                return BadRequest("The detector attached to the location of the job is busy");

            job.Name = req.Body.Name;
            // TODO(rg): if there are no tasks
            job.Type = req.Body.Type!.Value;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}