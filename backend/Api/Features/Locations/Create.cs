using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Locations
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public Create(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public class Req
        {
            public string Name { get; set; } = default!;
        }

        public class Validator : AbstractValidator<Req>
        {
            public Validator() => RuleFor(l => l.Name).Length(3, 64);
        }

        public class Res
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
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
            if (_context.Locations.Any(l => l.Name == req.Name))
            {
                return BadRequest("Location with the same name already exists");
            }

            var location = new Location
            {
                Name = req.Name
            };

            await _context.Locations.AddAsync(location, ct);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(nameof(Routes.Locations.GetById), new { location.Id }, _mapper.Map<Res>(location));

        }
    }
}