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

namespace Api.Features.Locations
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

        public Create(Context context, IMapper mapper, IValidator<Req> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;
            public Validator(Context context)
            {
                _context = context;

                RuleFor(l => l.Name).MaximumLength(64).NotEmpty();
                RuleFor(l => l).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the Station.");

            }
            private bool HaveUniqueNameWithinParent(Req req) =>
                _context.Locations.Where(l => l.StationId == req.ParentId).All(l => l.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Location, Res>();
            }
        }

        [HttpPost(Routes.Locations.Create)]
        [SwaggerOperation(
            Summary = "Create new location",
            Description = "Create new location",
            OperationId = "Locations.Create",
            Tags = new[] { "Locations" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var parentStation = await _context.Stations
                .Include(s => s.Locations)
                .SingleOrDefaultAsync(s => s.Id == req.ParentId, ct);

            if (parentStation is null)
                return BadRequest("Parent Station does not exist!");

            var location = new Location
            {
                Name = req.Name
            };

            parentStation.Locations.Add(location);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Locations.GetById, new { location.Id }, _mapper.Map<Res>(location));
        }
    }
}