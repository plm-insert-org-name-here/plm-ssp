using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Application.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Sites
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;
        private readonly IValidator<Req> _validator;

        public record Req(string Name);
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
                RuleFor(s => s).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
            }

            private bool HaveUniqueName(Req req) => _context.Sites.All(s => s.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Site, Res>();
            }
        }

        [HttpPost(Routes.Sites.Create)]
        [SwaggerOperation(
            Summary = "Create new site",
            Description = "Create new site",
            OperationId = "Sites.Create",
            Tags = new[] { "Sites" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var site = new Site
            {
                Name = req.Name
            };

            await _context.Sites.AddAsync(site, ct);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Locations.GetById, new { site.Id },
                _mapper.Map<Res>(site));
        }
    }
}