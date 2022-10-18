using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Application.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Sites
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
                RuleFor(s => s).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
            }

            private bool HaveUniqueName(Req req) => _context.Sites.All(s => s.Name != req.Body.Name);
        }

        [HttpPut(Routes.Sites.Update)]
        [SwaggerOperation(
            Summary = "Update an existing site",
            Description = "Update an existing site",
            OperationId = "Sites.Update",
            Tags = new[] { "Sites" })
        ]
        public override async Task<ActionResult> HandleAsync([FromRoute] Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var site = await _context.Sites.FindAsync(new object[] { req.Id }, ct);
            if (site is null) return NotFound();

            site.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}