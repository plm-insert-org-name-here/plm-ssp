using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Lines
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;
        private readonly IValidator<Req> _validator;

        public record Req(int ParentId, string Name);
        public record Res(int Id, string Name);

        public Create(Context context, IValidator<Req> validator, IMapper mapper)
        {
            _context = context;
            _validator = validator;
            _mapper = mapper;
        }

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;

            public Validator(Context context)
            {
                _context = context;
                
                RuleFor(l => l.Name).MaximumLength(64).NotEmpty();
                RuleFor(l => l).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the OPU.");
            }

            private bool HaveUniqueNameWithinParent(Req req) =>
                _context.Lines.Where(l => l.OPUId == req.ParentId).All(l => l.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Line, Res>();
            }
        }

        [HttpPost(Routes.Lines.Create)]
        [SwaggerOperation(
            Summary = "Create new Line",
            Description = "Create new Line",
            OperationId = "Lines.Create",
            Tags = new[] { "Lines" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var parentOPU = await _context.OPUs
                .Include(o => o.Lines)
                .SingleOrDefaultAsync(o => o.Id == req.ParentId, ct);

            if (parentOPU is null)
                return BadRequest("Parent OPU does not exist!");

            var line = new Line
            {
                Name = req.Name,
            };

            parentOPU.Lines.Add(line);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Locations.GetById, new { line.Id },
                _mapper.Map<Res>(line));
        }
    }
}