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

namespace Api.Features.OPUs
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
                RuleFor(s => s).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the Site.");
            }

            private bool HaveUniqueNameWithinParent(Req req)
            {
                var opu = _context.OPUs.SingleOrDefault(o => o.Id == req.Id);
                if (opu is null) return false;

                return _context.OPUs.Where(o => o.SiteId == opu.SiteId).All(o => o.Name != req.Body.Name);
            }
        }

        [HttpPut(Routes.OPUs.Update)]
        [SwaggerOperation(
            Summary = "Update an existing OPU",
            Description = "Update an existing OPU",
            OperationId = "OPUs.Update",
            Tags = new[] { "OPUs" })
        ]
        public override async Task<ActionResult> HandleAsync([FromRoute] Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var opu = await _context.OPUs.FindAsync(new object[] { req.Id }, ct);
            if (opu is null) return NotFound();

            opu.Name = req.Body.Name;
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}