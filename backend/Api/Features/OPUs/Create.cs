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

namespace Api.Features.OPUs
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
                
                RuleFor(s => s.Name).MaximumLength(64).NotEmpty();
                RuleFor(s => s).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the Site.");
            }

            private bool HaveUniqueNameWithinParent(Req req) =>
                _context.OPUs.Where(o => o.SiteId == req.ParentId).All(o => o.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<OPU, Res>();
            }
        }

        [HttpPost(Routes.OPUs.Create)]
        [SwaggerOperation(
            Summary = "Create new OPU",
            Description = "Create new OPU",
            OperationId = "OPUs.Create",
            Tags = new[] { "OPUs" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var parentSite = await _context.Sites
                .Include(s => s.OPUs)
                .SingleOrDefaultAsync(s => s.Id == req.ParentId, ct);

            if (parentSite is null)
                return BadRequest("Parent site does not exist!");

            var opu = new OPU
            {
                Name = req.Name,
            };

            parentSite.OPUs.Add(opu);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Locations.GetById, new { opu.Id },
                _mapper.Map<Res>(opu));
        }
    }
}